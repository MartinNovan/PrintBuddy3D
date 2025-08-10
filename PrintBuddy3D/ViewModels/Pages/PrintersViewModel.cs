using System;
using System.Collections.ObjectModel;
using System.IO;
using Avalonia.Media.Imaging;
using PrintBuddy3D.Models;

namespace PrintBuddy3D.ViewModels.Pages;

public class PrintersViewModel
{
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
    
}