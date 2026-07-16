using System.Collections.ObjectModel;
using System.Text.Json;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using SukiUI.Toasts;
using PrintBuddy3D.Services;

namespace PrintBuddy3D.ViewModels.Pages;

public class MenuItemDto
{
    public string? Title { get; set; }
    public string? Icon { get; set; } // Icon name (must be material icon)
    public string? Page { get; set; } // file name without .md
    public List<MenuItemDto>? Children { get; set; }
}

public partial class GuidesViewModel : PageBase
{
    [ObservableProperty]
    private bool _isErrorState = true; // defaults to error state to begin true till the structure loads
    private ObservableCollection<MenuItemDto> MenuStructure { get; } = new();

    private readonly ISukiToastManager _sukiToastManager; // Service for toast notification if wiki is not reachable
    private readonly IGuidesService _guidesService; // Service for downloading and updating local wiki 

    public event EventHandler<PageBase>? NavigationRequested;

    public GuidesViewModel(ISukiToastManager sukiToastManager, IGuidesService guidesService) : base("Guides", MaterialIconKind.Book, 4)
    {
        _sukiToastManager = sukiToastManager;
        _guidesService = guidesService;
        _ = InitializeGuidesAsync();
    }
    
    // Command for button if wiki does not load correctly
    [RelayCommand]
    private async Task RetrySyncAsync()
    {
        await InitializeGuidesAsync(true);
    }

    private async Task InitializeGuidesAsync(bool autoRedirect = false)
    {
        // Sync all cache to remote and return true if OK or false if something is wrong
        bool syncSuccess = await _guidesService.SyncAllOfflineDataAsync();
    
        if(!syncSuccess) // if something is wrong notify user
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
                _sukiToastManager.CreateToast()
                    .WithTitle("Offline Mode")
                    .WithContent("Could not sync latest guides with GitHub. Using local cache.")
                    .OfType(NotificationType.Warning)
                    .Dismiss().ByClicking()
                    .Queue()
            );
        }
        // Try to load structure (even if sync failed bcs we should have local copy)
        await LoadMenuStructureAsync(autoRedirect);
    }

    private async Task LoadMenuStructureAsync(bool autoRedirect = false)
    {
        // Get the JSON, either the remote or local (depends on multiple things) 
        var json = await _guidesService.GetWikiFileTextAsync("_Menu.json");

        // If we failed to get the JSON (either remote/local), or it is empty, the throw error state
        if (string.IsNullOrEmpty(json.Content))
        {
            IsErrorState = true;
            return; 
        }

        // If there is something in the file, it should be fine, turn off error state
        IsErrorState = false;
        
        // Get the pages from json
        var items = JsonSerializer.Deserialize<List<MenuItemDto>>(json.Content);
        // TODO check the structure of JSON if its correct (edge case)
        MenuStructure.Clear(); // Clear existing structure (shouldnt exists, its for future methods, that will update the wiki, by hand)
        if (items != null)
        {
            foreach (var item in items) MenuStructure.Add(item); // Add every item to collection
        }
    
        // Build the menu
        BuildMenu(); 
        
        // Try to redirect if this method was invoked from error button 
        if (autoRedirect)
        {
            AutoRedirectToHome();
        }
    }

    // Automatically redirect to the first subpage if no error
    public void AutoRedirectToHome()
    {
        if (SubPages.Count > 0 && !IsErrorState) NavigationRequested?.Invoke(this, SubPages[0]);
    }
    
    private void BuildMenu()
    {
        SubPages.Clear(); // Clear subpages of this page  
        foreach (var dto in MenuStructure)
        {
            SubPages.Add(CreateMenuItem(dto)); // Add new subpages from the menu structure, but firstly we need to build the page in method CreateMenuItem
        }
    }

    private PageBase CreateMenuItem(MenuItemDto dto)
    {
        // Default icon if the file doesnt have one (defined in _Menu.json file)
        var iconKind = MaterialIconKind.FileDocumentOutline; 
        // If item has icon and its correct icon, use that icon instead (defined in _Menu.json file)
        if (!string.IsNullOrEmpty(dto.Icon) && Enum.TryParse(dto.Icon, out MaterialIconKind parsedIcon))
        {
            iconKind = parsedIcon;
        }
        // Create the page viewmodel, pass service and other args
        var pageVm = new WikiPageViewModel(_guidesService ,dto.Title ?? "Unknown Name", iconKind, dto.Page ?? "");
        
        // if page has no child pages, return this page
        if (dto.Children is not { Count: > 0 }) return pageVm;
        // if page has child pages, recursively create them too
        foreach (var childDto in dto.Children)
        {
            pageVm.SubPages.Add(CreateMenuItem(childDto));
        }
        return pageVm;
    }
}