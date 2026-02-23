using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
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
using PrintBuddy3D.Enums;
using PrintBuddy3D.Services;
using PrintBuddy3D.Views.Pages.PrinterControlsDockView;

namespace PrintBuddy3D.ViewModels.Pages;

public partial class PrinterControlViewModel : ObservableObject
{
    public PrinterModel Printer { get; }
    public readonly IPrinterControlService PrinterControlService;
    private readonly Action _goBack;
    private readonly IDockSerializer _serializer;
    private readonly IFactory _factory;
    
    [ObservableProperty] private IRootDock? _layout;
    [ObservableProperty] private bool _isWebModeEnabled;
    [ObservableProperty] private bool _isWebModeSupported;
    [ObservableProperty] private string _errorMessage = "WebView does not load properly!";
    [ObservableProperty] private List<int> _baudrates = new() { 2400, 9600, 19200, 38400, 57600, 115200, 250000, 500000, 1000000 };
    [ObservableProperty] private List<string>? _ports;
    [ObservableProperty] private string? _selectedPort;
    [ObservableProperty] private int? _selectedBaudrate;


    public PrinterControlViewModel(PrinterModel printer, Action goBack)
    {
        _serializer = new DockSerializer(typeof(AvaloniaList<>));
        _factory = new DockFactory(this);
        Printer = printer;
        _goBack = goBack;
        IsWebModeSupported = Printer.Firmware == PrinterEnums.Firmware.Klipper;
        Ports = SerialPort.GetPortNames().OrderBy(p => p, StringComparer.OrdinalIgnoreCase).ToList();
        if (Baudrates.Contains(Printer.BaudRate))
        {
            SelectedBaudrate = Printer.BaudRate;
        }
        if (Printer.LastSerialPort != null && Ports.Contains(Printer.LastSerialPort))
        {
            SelectedPort = Printer.LastSerialPort;
        }

        PrinterControlService = printer.Firmware switch
        {
            PrinterEnums.Firmware.Marlin => new MarlinPrinterControlService(Printer),
            PrinterEnums.Firmware.Klipper => new KlipperPrinterControlService(Printer),
            _ => throw new NotSupportedException("Unsupported firmware")
        };
        
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
    
    partial void OnSelectedBaudrateChanged(int? value)
    {
        Printer.BaudRate = value ?? Printer.BaudRate;
    }

    partial void OnSelectedPortChanged(string? value)
    {
        Printer.LastSerialPort = value ?? Printer.LastSerialPort;
    }

    [RelayCommand]
    private void EmergencyStop()
    {
        PrinterControlService.EmergencyStop();
    }
    
    [RelayCommand]
    private void Back()
    {
        PrinterControlService?.Dispose();
        _goBack();
    }
    [RelayCommand]
    private void SwitchModes()
    {
        IsWebModeEnabled = !IsWebModeEnabled;
    }

    [RelayCommand]
    private void ResetLayout()
    {
        Layout = _factory.CreateLayout();
        if (Layout != null)
        {
            _factory.InitLayout(Layout);
            if (Layout is { } root)
            {
                root.Navigate.Execute("Home");
            }
        }
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
                    _factory.InitLayout(layout);
                    Layout = layout;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
        
    private static List<FilePickerFileType> GetOpenLayoutFileTypes() => [ StorageService.Json, StorageService.All];
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