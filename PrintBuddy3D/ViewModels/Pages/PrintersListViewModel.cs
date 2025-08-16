using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using PrintBuddy3D.Models;
using SukiUI.Dialogs;

namespace PrintBuddy3D.ViewModels.Pages;

public partial class PrintersListViewModel : ObservableObject
{
    private readonly ISukiDialogManager _dialogManager;
    public ObservableCollection<PrinterModel> Printers { get; set; } = new()
    {
        new PrinterModel
        {
            Name = "Printer 1",
            Firmware = "Klipper",
            ConnectionType = "WiFi",
            DbHash = 123456,
            Address = "http://klipper.local",
        },
        new PrinterModel
        {
            Name = "Printer 1",
            Firmware = "Klipper",
            ConnectionType = "WiFi",
            DbHash = 123456,
            Address = "http://klipper1.local",
        },
        new PrinterModel
        {
            Name = "Printer 2",
            Firmware = "Marlin",
            ConnectionType = "USB",
            SerialPort = "Com3",
            DbHash = 654321,
        },
        new PrinterModel
        {
            Name = "Printer 2",
            Firmware = "Marlin",
            ConnectionType = "USB",
            SerialPort = "Com3",
            DbHash = 654321,
        },new PrinterModel
        {
            Name = "Printer 2",
            Firmware = "Marlin",
            ConnectionType = "USB",
            SerialPort = "Com3",
            DbHash = 654321,
        },new PrinterModel
        {
            Name = "Printer 2",
            Firmware = "Marlin",
            ConnectionType = "USB",
            SerialPort = "Com3",
            DbHash = 654321,
        },new PrinterModel
        {
            Name = "Printer 2",
            Firmware = "Marlin",
            ConnectionType = "USB",
            SerialPort = "Com3",
            DbHash = 654321,
        }
    };
    [ObservableProperty]
    private object? _currentContent;
    
    public PrintersListViewModel(ISukiDialogManager dialogManager)
    {
        _dialogManager = dialogManager;
        CurrentContent = this;
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
        if (printer.Address is null || printer.Firmware != "Klipper")
        {
            _dialogManager.CreateDialog()
                .OfType(NotificationType.Error)
                .WithTitle("SSH Connection Error")
                .WithContent("SSH connection is only available for Klipper firmware printers with a valid address. Please check the printer settings.")
                .WithActionButton("Dismiss", _ => { }, true)
                .Dismiss().ByClickingBackground()
                .TryShow();
            return;
        }

        ProcessStartInfo psi = new ProcessStartInfo();
        if (OperatingSystem.IsWindows())
        {
            psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoExit -Command \"ssh klipper@{printer.Address}\"",
                UseShellExecute = false
            };
        }
        else if (OperatingSystem.IsLinux())
        {
            psi = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"ssh user@{printer.Address}\"",
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
}