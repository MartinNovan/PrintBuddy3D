using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using PrintBuddy3D.Models;
using PrintBuddy3D.Services;
using SukiUI.Dialogs;

namespace PrintBuddy3D.ViewModels.Pages;

public partial class PrintersListViewModel : ObservableObject
{
    private readonly ISukiDialogManager _dialogManager;
    private DispatcherTimer? _refreshTimer;
    private readonly IPrintersService _printersService;
    
    [ObservableProperty] private ObservableCollection<PrinterModel> _printers;
    [ObservableProperty] private object? _currentContent;
    
    public PrintersListViewModel(ISukiDialogManager dialogManager, IPrintersService printersService)
    {
        _dialogManager = dialogManager;
        _printersService = printersService;
        CurrentContent = this;
        _ = LoadPrinters();
        foreach (var printerModel in Printers.Where(p => p.Status == "Offline"))
            printerModel.Status = "Online";
        Thread.Sleep(200);
        foreach (var printerModel in Printers.Where(p => p.Status == "Idle"))
            printerModel.Status = "Offline";
        Thread.Sleep(200);
        foreach (var printerModel in Printers.Where(p => p.Status == "Done"))
            printerModel.Status = "Idle";
        Thread.Sleep(200);
        foreach (var printerModel in Printers.Where(p => p.Status == "Printing"))
            printerModel.Status = "Done";
    }

    
    [RelayCommand]
    private void OpenPrinterControl(PrinterModel printer)
    {
        CurrentContent = new PrinterControlViewModel(printer, GoBack);
    }
    
    [RelayCommand]
    private void OpenPrinterEditor(PrinterModel? printer)
    {
        CurrentContent = new PrinterEditorViewModel(printer, GoBack);
    }
    
    [RelayCommand]
    private void SshIntoPrinter(PrinterModel printer)
    {
        var dialog = _dialogManager.CreateDialog()
            .OfType(NotificationType.Error)
            .WithTitle("SSH Connection Error")
            .WithActionButton("Dismiss", _ => { }, true)
            .Dismiss().ByClickingBackground();
        if (String.IsNullOrEmpty(printer.Address) || printer.Firmware != "Klipper")
        {
            dialog.WithContent("SSH connection is only available for Klipper firmware printers with a valid address. Please check the printer settings.").TryShow();
            return;
        }

        if (String.IsNullOrEmpty(printer.HostUserName))
        {
            dialog.WithContent("SSH connection requires a valid host username. Please set the host username in the printer settings.").TryShow();
            return;
        }

        ProcessStartInfo psi = new ProcessStartInfo();
        if (OperatingSystem.IsWindows())
        {
            psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoExit -Command \"ssh {printer.HostUserName}@{printer.Address.Replace("https://", "").Replace("http://", "")}\"",
                UseShellExecute = false
            };
        }
        else if (OperatingSystem.IsLinux())
        {
            psi = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"ssh {printer.HostUserName}@{printer.Address}\"",
                UseShellExecute = false
            };
        }
        Process.Start(psi);
    }
    
    [RelayCommand]
    private void RemovePrinter(PrinterModel printer)
    {
        if (Printers.Contains(printer))
        {
            _dialogManager.CreateDialog()
                .OfType(NotificationType.Warning)
                .WithTitle("Removing Printer")
                .WithContent($"Are you sure you want to remove the printer '{printer.Name}'?")
                .WithActionButton("Yes", _ =>
                {
                    Printers.Remove(printer);
                    //TODO: Implement remove logic from database
                }, true).WithActionButton("No", _ => { }, true)
                .Dismiss().ByClickingBackground()
                .TryShow();
        }
    }

    private void GoBack()
    {
        CurrentContent = App.Services.GetRequiredService<PrintersListViewModel>(); 
    }
    
    private void Printer_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        try
        {
            if (e.PropertyName == nameof(PrinterModel.Status))
            {
                if (_refreshTimer == null)
                {
                    _refreshTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) }; // 100ms interval to prevent UI collapse during rapid updates
                    _refreshTimer.Tick += (s, args) =>
                    {
                        _refreshTimer.Stop();
                        _refreshTimer = null;
                        // Functions to refresh UI on Home page
                        App.Services.GetRequiredService<HomeViewModel>().RefreshPrintersCount();
                        var printer = (PrinterModel)sender!;
                        App.Services.GetRequiredService<HomeViewModel>().RecieveNotification(printer);
                    };
                }
                _refreshTimer.Stop();
                _refreshTimer.Start();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    private async Task LoadPrinters()
    {
        var printers = await _printersService.GetPrintersAsync();
        foreach (var printer in printers)
        {
            printer.PropertyChanged += Printer_PropertyChanged;
        }
        Printers = printers;
    }
}