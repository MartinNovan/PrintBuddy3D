using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using PrintBuddy3D.Models;
using SukiUI.Controls;

namespace PrintBuddy3D.ViewModels.Pages;

public partial class PrintersListViewModel : ObservableObject, ISukiStackPageTitleProvider
{
    public string Title { get; } = "Printers List";
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
    private object? currentContent;
    
    public PrintersListViewModel()
    {
        CurrentContent = this; 
    }
    
    [RelayCommand]
    private void OpenPrinterControl(PrinterModel printer)
    {
        CurrentContent = new PrinterControlViewModel(printer, GoBack);
    }

    private void GoBack()
    {
        CurrentContent = App.Services.GetRequiredService<PrintersListViewModel>(); 
    }
}