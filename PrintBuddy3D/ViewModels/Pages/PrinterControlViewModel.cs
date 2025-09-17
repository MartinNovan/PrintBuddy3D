using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dock.Model.Controls;
using Dock.Model.Core;
using PrintBuddy3D.Models;
using Dock.Serializer;
using PrintBuddy3D.Controls;
using PrintBuddy3D.Views.Pages.PrinterControlsView;
using WebviewGtk;

namespace PrintBuddy3D.ViewModels.Pages;

public partial class PrinterControlViewModel : ObservableObject
{
    public PrinterModel Printer { get; }
    private readonly Action _goBack;
    private readonly IDockSerializer _serializer;
    private readonly IFactory _factory;
    
    [ObservableProperty] private IRootDock? _layout;
    [ObservableProperty] private bool _isWebModeEnabled;
    [ObservableProperty] private string _errorMessage = "WebView does not load properly!";
    public PrinterControlViewModel(PrinterModel printer, Action goBack)
    {
        _serializer = new DockSerializer(typeof(AvaloniaList<>));
        _factory = new DockFactory(this);
        Printer = printer;
        _goBack = goBack;
        
        Layout = _factory.CreateLayout();

        if (Layout is null)
        {
            return;
        }

        _factory.InitLayout(Layout);

        if (Layout is { } root)
        {
            root.Navigate.Execute("Home");
        }
    }
    
    [RelayCommand]
    private void Back()
    {
        _goBack();
    }
    
    
        [RelayCommand]
        private async Task SaveLayout()
        {
            var storageProvider = StorageService.GetStorageProvider();

            var file = await storageProvider!.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save layout",
                FileTypeChoices = GetOpenLayoutFileTypes(),
                SuggestedFileName = "layout",
                DefaultExtension = "json",
                ShowOverwritePrompt = true
            });

            if (file is not null)
            {
                try
                {
                    await using var stream = await file.OpenWriteAsync();

                    if (Layout is not null)
                    {
                        _serializer.Save(stream, Layout);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        [RelayCommand]
        private async Task OpenLayout()
        {
            var storageProvider = StorageService.GetStorageProvider();

            var result = await storageProvider!.OpenFilePickerAsync(
                new FilePickerOpenOptions
                {
                    Title = "Open layout",
                    FileTypeFilter = GetOpenLayoutFileTypes(),
                    AllowMultiple = false
                });

            var file = result.FirstOrDefault();

            if (file is not null)
            {
                try
                {
                    await using var stream = await file.OpenReadAsync();
                    using var reader = new StreamReader(stream);

                    var layout = _serializer.Load<IRootDock?>(stream);

                    if (layout is not null)
                    {
                        _factory!.InitLayout(layout);
                        Layout = layout;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
        
        private static List<FilePickerFileType> GetOpenLayoutFileTypes()
            =>
            [
                StorageService.Json,
                StorageService.All
            ];

    [RelayCommand]
    private void SwitchModes()
    {
        //TODO: find suitable webview for linux or find other solution to open web interface in app
        IsWebModeEnabled = !IsWebModeEnabled;
        if (OperatingSystem.IsLinux() && IsWebModeEnabled)
        {
            try
            {
                //WebkitGtkWrapper.RunWebkit(new Uri(Printer.FullAddress));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error when opening URL: {ex.Message}");
            }
        }
    }
    
    internal static class StorageService
    {
        public static FilePickerFileType All { get; } = new("All")
        {
            Patterns = ["*.*"],
            MimeTypes = ["*/*"]
        };

        public static FilePickerFileType Json { get; } = new("Json")
        {
            Patterns = ["*.json"],
            AppleUniformTypeIdentifiers = ["public.json"],
            MimeTypes = ["application/json"]
        };

        public static IStorageProvider? GetStorageProvider()
        {
            if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime { MainWindow: { } window })
            {
                return window.StorageProvider;
            }

            if (Avalonia.Application.Current?.ApplicationLifetime is ISingleViewApplicationLifetime 
                {
                    MainView: { } mainView
                })
            {
                var visualRoot = mainView.GetVisualRoot();
                if (visualRoot is TopLevel topLevel)
                {
                    return topLevel.StorageProvider;
                }
            }

            return null;
        }
    }
}