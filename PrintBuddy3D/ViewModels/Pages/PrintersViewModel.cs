using System.Collections.ObjectModel;
using PrintBuddy3D.Models;

namespace PrintBuddy3D.ViewModels.Pages;

public class PrintersViewModel
{
    public ObservableCollection<PrinterModel> Printers { get; set; } = new ObservableCollection<PrinterModel>()
    {
        new PrinterModel
        {
            Name = "Printer 1",
            Firmware = "Klipper",
            ConnectionType = "WiFi",
            DbHash = 123456,
            Address = "http://klipper.local"
        },
        new PrinterModel
        {
            Name = "Printer 2",
            Firmware = "Marlin",
            ConnectionType = "USB",
            SerialPort = "Com3",
            DbHash = 654321
        },
        new PrinterModel
        {
            Name = "Printer 2",
            Firmware = "Marlin",
            ConnectionType = "USB",
            SerialPort = "Com3",
            DbHash = 654321
        },new PrinterModel
        {
            Name = "Printer 2",
            Firmware = "Marlin",
            ConnectionType = "USB",
            SerialPort = "Com3",
            DbHash = 654321
        },new PrinterModel
        {
            Name = "Printer 2",
            Firmware = "Marlin",
            ConnectionType = "USB",
            SerialPort = "Com3",
            DbHash = 654321
        },new PrinterModel
        {
            Name = "Printer 2",
            Firmware = "Marlin",
            ConnectionType = "USB",
            SerialPort = "Com3",
            DbHash = 654321
        }
    };
    
}