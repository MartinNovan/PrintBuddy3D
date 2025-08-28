﻿using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using PrintBuddy3D.Enums;
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
        if (String.IsNullOrEmpty(printer.Address) || printer.Firmware != PrinterEnums.Firmware.Klipper)
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
    

    private async Task LoadPrinters()
    {
        var printers = await _printersService.GetPrintersAsync();
        Printers = printers;
    }
}