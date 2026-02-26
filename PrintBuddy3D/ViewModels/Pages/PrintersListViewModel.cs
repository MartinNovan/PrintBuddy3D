using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PrintBuddy3D.Enums;
using PrintBuddy3D.Models;
using PrintBuddy3D.Services;
using PrintBuddy3D.Views;
using SukiUI.Dialogs;

namespace PrintBuddy3D.ViewModels.Pages;

public partial class PrintersListViewModel : ObservableObject
{
    private readonly ISukiDialogManager _dialogManager;
    private readonly IPrintersService _printersService;
    private readonly IPrinterMonitoringService _printerMonitoringService;

    [ObservableProperty] private ObservableCollection<PrinterModel>? _printers;
    [ObservableProperty] private object? _currentContent;
    
    public PrintersListViewModel(ISukiDialogManager dialogManager, IPrintersService printersService, IPrinterMonitoringService printerMonitoringService)
    {
        _dialogManager = dialogManager;
        _printersService = printersService;
        _printerMonitoringService = printerMonitoringService;
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
        CurrentContent = new PrinterEditorViewModel(printer, _printersService, GoBack);
    }
    
    [RelayCommand]
    private void SshIntoPrinter(PrinterModel printer)
    {
        try
        {
            var dialog = _dialogManager.CreateDialog()
                .OfType(NotificationType.Error)
                .WithTitle("SSH Connection Error")
                .WithActionButton("Dismiss", _ => { }, true)
                .Dismiss().ByClickingBackground();
            if (String.IsNullOrEmpty(printer.Address) || printer.Firmware != PrinterEnums.Firmware.Klipper)
            {
                dialog.WithContent(
                        "SSH connection is only available for Klipper firmware printers with a valid address. Please check the printer settings.")
                    .TryShow();
                return;
            }

            if (String.IsNullOrEmpty(printer.HostUserName))
            {
                dialog.WithContent(
                        "SSH connection requires a valid host username. Please set the host username in the printer settings.")
                    .TryShow();
                return;
            }
            
            var vm = new SshWindowViewModel(printer.Address.Replace("https://", "").Replace("http://", ""), printer.HostUserName);
            var window = new SshWindow { DataContext = vm };
            window.Show();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"There was an issue opening a terminal: {ex.Message}");
        }
    }

    [RelayCommand]
    private void RemovePrinter(PrinterModel printer)
    {
        if (Printers != null && Printers.Contains(printer))
        {
            _dialogManager.CreateDialog()
                .OfType(NotificationType.Warning)
                .WithTitle("Removing Printer")
                .WithContent($"Are you sure you want to remove the printer '{printer.Name}'?")
                .WithActionButton("Yes", _ =>
                {
                    _printersService.RemovePrinterAsync(printer);
                    Printers.Remove(printer);
                }, true).WithActionButton("No", _ => { }, true)
                .Dismiss().ByClickingBackground()
                .TryShow();
        }
    }

    private void GoBack()
    {
        CurrentContent = this; 
    }
    

    private async Task LoadPrinters()
    {
        Printers = await _printersService.GetPrintersAsync();
        _printerMonitoringService.Start(Printers);
    }
}