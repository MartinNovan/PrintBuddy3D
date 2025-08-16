using System;
using System.Collections.ObjectModel;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dock.Model.Core;
using Microsoft.Extensions.DependencyInjection;
using PrintBuddy3D.Models;
using PrintBuddy3D.Views;
using PrintBuddy3D.Views.Pages;
using SukiUI.Controls;
using SukiUI.Controls.Experimental.DesktopEnvironment;

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
            ImagePath = "C:\\Users\\mnova\\Documents\\GitHub\\PrintBuddy3D\\PrintBuddy3D\\Assets\\Pictures\\klipper-logo.png"
        },
        new PrinterModel
        {
            Name = "Printer 2",
            Firmware = "Marlin",
            ConnectionType = "USB",
            SerialPort = "Com3",
            DbHash = 654321,
            ImagePath = "C:\\Users\\mnova\\Documents\\GitHub\\PrintBuddy3D\\PrintBuddy3D\\Assets\\Pictures\\marlin-outrun-nf-500.png"
        },
        new PrinterModel
        {
            Name = "Printer 2",
            Firmware = "Marlin",
            ConnectionType = "USB",
            SerialPort = "Com3",
            DbHash = 654321,
            ImagePath = "C:\\Users\\mnova\\Documents\\GitHub\\PrintBuddy3D\\PrintBuddy3D\\Assets\\Pictures\\marlin-outrun-nf-500.png"
        },new PrinterModel
        {
            Name = "Printer 2",
            Firmware = "Marlin",
            ConnectionType = "USB",
            SerialPort = "Com3",
            DbHash = 654321,
            ImagePath = "C:\\Users\\mnova\\Documents\\GitHub\\PrintBuddy3D\\PrintBuddy3D\\Assets\\Pictures\\marlin-outrun-nf-500.png"
        },new PrinterModel
        {
            Name = "Printer 2",
            Firmware = "Marlin",
            ConnectionType = "USB",
            SerialPort = "Com3",
            DbHash = 654321,
            ImagePath = "C:\\Users\\mnova\\Documents\\GitHub\\PrintBuddy3D\\PrintBuddy3D\\Assets\\Pictures\\marlin-outrun-nf-500.png"
        },new PrinterModel
        {
            Name = "Printer 2",
            Firmware = "Marlin",
            ConnectionType = "USB",
            SerialPort = "Com3",
            DbHash = 654321,
            ImagePath = "C:\\Users\\mnova\\Documents\\GitHub\\PrintBuddy3D\\PrintBuddy3D\\Assets\\Pictures\\marlin-outrun-nf-500.png"
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