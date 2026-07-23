using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using PrintBuddy3D.Models;
using PrintBuddy3D.Services;
using PrintBuddy3D.Views.Pages.Dialogs;
using SukiUI.Dialogs;

namespace PrintBuddy3D.ViewModels.Pages;

// Sorting option
public enum SortOption { Name, Date, Size }

public partial class FilesViewModel : PageBase, IDisposable
{
    private readonly PrintersListViewModel _printersViewModel; // VM to get the printer list
    private readonly IFilesService _filesService; // Service for working with the files (getting )
    private FileSystemWatcher? _watcher; // File watcher, that watches changes in dir and all subdir
    private bool _isReloading; // boolean to stop spam call from file watcher
    private readonly string _rootDirectory; // Root dir of files //TODO make this editable in settings and get rid of appdata service
    private readonly IAppDataService _appDataService; // Appdata service to currently get appdata directory (will not be needed after making root dir editable) 
    private readonly ISukiDialogManager _dialogManager; // Suki dialog manager to show dialogs

    // all files that we found in lists
    private List<FileModel> _models = [];
    private List<FileModel> _projects = [];
    private List<FileModel> _gcodes = [];

    // UI collections for showing filtered files
    public ObservableCollection<FileModel> ModelsList { get; } = [];
    public ObservableCollection<FileModel> ProjectsList { get; } = [];
    public ObservableCollection<FileModel> GcodesList { get; } = [];
    
    
    [ObservableProperty] private string _searchText = "";
    [ObservableProperty] private SortOption _selectedSortOption = SortOption.Name;
    [ObservableProperty] private bool _isPrinterMenuShown;
    
    // List of avaiable sort options
    public List<SortOption> SortOptions { get; } = Enum.GetValues<SortOption>().ToList();

    public FilesViewModel(ISukiDialogManager sukiDialogManager,IAppDataService appDataService, IFilesService filesService, PrintersListViewModel printersListViewModel) : base("Files", MaterialIconKind.FolderOutline, 3)
    {
        _dialogManager = sukiDialogManager;
        _appDataService = appDataService;
        _filesService = filesService;
        _printersViewModel = printersListViewModel;
        _rootDirectory = Path.Combine(appDataService.GetAppBasePath(), "PrintBuddyLibrary");
        
        if (!Directory.Exists(_rootDirectory)) Directory.CreateDirectory(_rootDirectory); // Create root dir if not exists
        InitializeWatcher(); // Start the file watcher
        _ = LoadFilesAsync(); // Load all files
    }

    // Apply filtering when search text changes
    partial void OnSearchTextChanged(string value) => ApplyFiltersAndSort();

    // Apply sorting when we change the selected value
    partial void OnSelectedSortOptionChanged(SortOption value) => ApplyFiltersAndSort();

    
    [RelayCommand]
    // Opens the file directory
    private void OpenFileLocation(FileModel? file)
    {
        if (file == null || !File.Exists(file.FilePath)) return; // if file is null or doesnt exists return
        try
        {
            var directory = Path.GetDirectoryName(file.FilePath)!; // Get the dir name (instead of the file) 
            // Open the directory
            Process.Start(new ProcessStartInfo
            {
                FileName = directory,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error when opening folder: {ex.Message}");
        }
    }

    [RelayCommand]
    // Method for deleting a file
    private void DeleteFile(FileModel? file)
    {
        if (file == null || !File.Exists(file.FilePath)) return; // if file is null or doesnt exists return
        try
        {
            File.Delete(file.FilePath); // Delete the file
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error when deleting a file: {ex.Message}");
        }
    }

    [RelayCommand]
    // Method for executing/opening the file
    private void ExecuteFile(FileModel? file)
    {
        if (file == null || !File.Exists(file.FilePath)) return; // if file is null or doesnt exists return
        // Execute the file (opens in default program)
        Process.Start(new ProcessStartInfo
        {
            FileName = file.FilePath,
            UseShellExecute = true
        });
    }

    [RelayCommand]
    // Method for sending gcode with starting the print
    private void PrintFileOnPrinter(FileModel? file)
    {
        ShowPrinterDialogAndUpload(file, startPrint: true);
    }
    
    [RelayCommand]
    // Method for sending gcode without starting the print
    private void SendFileToPrinter(FileModel? file)
    {
        ShowPrinterDialogAndUpload(file, startPrint: false);
    }
    
    // Method for handling sending the files
    private void ShowPrinterDialogAndUpload(FileModel? file, bool startPrint)
    {
        if (file == null) return; // if file is null, return

        var realPrinters = _printersViewModel.Printers; // Get printers from _printersViewModel

        // Create new dialog for prompting the user on which printers is file send to  
        var dialogContent = new PrinterSelectionDialog
        {
            DataContext = realPrinters
        };
        // Dynamic dialog title that depends on if we wanted the file to be printed or only upload
        var dialogTitle = startPrint ? "Select printers to print on" : "Select printers to upload to";

        // Creating dialog with suki dialog manager
        _dialogManager.CreateDialog()
            .WithTitle(dialogTitle)
            .WithContent(dialogContent)
            .WithActionButton("Cancel", _ => { }, true) // Cancel button that only dismisses the dialog
            .WithActionButton("Send", _ => 
            {
                // get the selected printers from method from dialog
                var selectedPrinters = dialogContent.GetSelectedPrinters();
                // if user didnt select any printer, return
                if (selectedPrinters.Count == 0) return;

                Task.Run(async () => 
                {
                    // Send info to the service, which file we want to send, to what printers and if we want to start print, method will return results
                    var results = await _filesService.UploadToPrintersAsync(file.FilePath, selectedPrinters, startPrint);
                    // TODO maybe make some notification that it was succesfull or failed (probably use the suki toast manager but effectively)
                });
            }, true)
            .TryShow();
    }

    // Method for starting the watcher
    private void InitializeWatcher()
    {
        _watcher = new FileSystemWatcher(_rootDirectory)
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite, 
            IncludeSubdirectories = true,
            EnableRaisingEvents = true
        };
        _watcher.Created += OnFileSystemChanged;
        _watcher.Deleted += OnFileSystemChanged;
        _watcher.Renamed += OnFileSystemChanged;
    }

    // Method for updating the file list if watcher catched some change
    private void OnFileSystemChanged(object sender, FileSystemEventArgs e)
    {
        if (_isReloading) return; // if we already loading the change, return
        _isReloading = true; // Set to true to block spam calling
        Task.Run(async () =>
        {
            await Task.Delay(500); // Wait a while
            await LoadFilesAsync(); // load file
            _isReloading = false; 
        });
    }

    // Method for loading all files we want
    private async Task LoadFilesAsync()
    {
        // Call service for each collection and get list of filemodels
        _models = await _filesService.GetFilesInDirectoryAsync(_rootDirectory, [".stl", ".obj", ".step", ".stp", ".amf", ".drc", ".svg"]);
        _projects = await _filesService.GetFilesInDirectoryAsync(_rootDirectory, [".3mf"]);
        _gcodes = await _filesService.GetFilesInDirectoryAsync(_rootDirectory, [".gcode", ".bgcode"]);
        
        // Filter these data on UI thread
        Dispatcher.UIThread.Post(ApplyFiltersAndSort);
        
        // prepare cache dir for thumbnails 
        var cacheDir = Path.Combine(_appDataService.GetAppBasePath(), "Cache", "Thumbnails");
        LoadThumbnailsAsync(_gcodes, _projects, cacheDir); // load thumbnails for project and gcode files using the cache dir
    }

    // Method for loading thumbnails for project and gcode files and using cache dir
    private void LoadThumbnailsAsync(List<FileModel> gcodes, List<FileModel> projects, string cacheDir)
    {
        // For each gcode file
        foreach (var file in gcodes)
        {
            Task.Run(async () =>
            {
                // Call service to get the thumbnail
                var thumbPath = await _filesService.ExtractThumbnailFromGcodeAsync(file.FilePath, cacheDir);
                // If thumbnail isnt null call ui thread to show the image
                if (thumbPath != null) Dispatcher.UIThread.Post(() => file.ThumbnailPath = thumbPath);
            });
        }
        // For each project file
        foreach (var file in projects)
        {
            Task.Run(async () =>
            {
                // Call service to get the thumbnail
                var thumbPath = await _filesService.ExtractThumbnailFromProjectAsync(file.FilePath, cacheDir);
                // If thumbnail isnt null call ui thread to show the image
                if (thumbPath != null) Dispatcher.UIThread.Post(() => file.ThumbnailPath = thumbPath);
            });
        }
    }
    
    // Method for updating all ui collection using filter
    private void ApplyFiltersAndSort()
    {
        UpdateCollection(ModelsList, _models);
        UpdateCollection(ProjectsList, _projects);
        UpdateCollection(GcodesList, _gcodes);
    }

    // Method for updating one collection using filter
    private void UpdateCollection(ObservableCollection<FileModel> targetCollection, List<FileModel> rawFiles)
    {
        var query = rawFiles.AsEnumerable(); // get all files as enumerable

        // filter using the search text if not null/empty
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            query = query.Where(f => f.FileName.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
        }

        // sort the collection
        query = SelectedSortOption switch
        {
            SortOption.Date => query.OrderByDescending(f => f.LastModified),
            SortOption.Size => query.OrderByDescending(f => f.FileSize),
            _ => query.OrderBy(f => f.FileName)
        };

        // Clear old colleciton
        targetCollection.Clear();
        // add every element from newely filtered to UI collection 
        foreach (var item in query)
        {
            targetCollection.Add(item);
        }
    }

    // Method for diposing the watcher
    public void Dispose()
    {
        if (_watcher == null) return;
        _watcher.EnableRaisingEvents = false;
        _watcher.Dispose();
    }
}