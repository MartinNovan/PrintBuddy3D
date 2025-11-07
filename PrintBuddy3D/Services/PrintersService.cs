using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using PrintBuddy3D.Enums;
using PrintBuddy3D.Models;

namespace PrintBuddy3D.Services;

public interface IPrintersService
{
    Task<ObservableCollection<PrinterModel>> GetPrintersAsync();
    Task<PrinterEnums.Status> GetPrinterStatusAsync();
}

public interface IPrinterControlService
{
    void SendCommand(string command);
    void Move();
    void Home();
    void SetTemperature(int temp);
}
public class PrintersService(IAppDataService appDataService, INotificationService notificationService) : IPrintersService
{
    private readonly SqliteConnection _dbConnection = appDataService.DbConnection;
    public Task<ObservableCollection<PrinterModel>> GetPrintersAsync()
    {
        var printers = new ObservableCollection<PrinterModel>()
        {
            new()
            {
                Name = "Klipper 1",
                Firmware = PrinterEnums.Firmware.Klipper,
                Address = "klipper.local",
                Prefix = PrinterEnums.Prefix.Http,
                HostUserName = "klipper",
            },
            new()
            {
                Name = "Klipper 2",
                Firmware = PrinterEnums.Firmware.Klipper,
                Prefix = PrinterEnums.Prefix.Http,
                Address = "10.0.0.88",
                HostUserName = "klipper",
            },
            new()
            {
                Name = "Klipper 3",
                Firmware = PrinterEnums.Firmware.Klipper,
                Prefix = PrinterEnums.Prefix.Https,
                Address = "google.com",
                HostUserName = "marvy",
            },
            new()
            {
                Name = "Marlin 1",
                Firmware = PrinterEnums.Firmware.Marlin,
                LastSerialPort = "Com3",
            },
            new()
            {
                Name = "Marlin 2",
                Firmware = PrinterEnums.Firmware.Marlin,
                LastSerialPort = "Com3",
            },
            new()
            {
                Name = "Marlin 3",
                Firmware = PrinterEnums.Firmware.Marlin,
                LastSerialPort = "Com3",
            }
        };
        foreach (var printer in printers)
        {
            printer.PropertyChanged += Printer_PropertyChanged;
        }
        
        return Task.FromResult(printers);
    }

    public Task<PrinterEnums.Status> GetPrinterStatusAsync()
    {
        var status = Random.Shared.Next(0, 3);
        switch (status)
        {
            case 0:
                return Task.FromResult(PrinterEnums.Status.StandBy);
            case 1:
                return Task.FromResult(PrinterEnums.Status.Offline);
            case 2:
                return Task.FromResult(PrinterEnums.Status.Printing);
            case 3:
                return Task.FromResult(PrinterEnums.Status.Complete);
        }
        return Task.FromResult(PrinterEnums.Status.Error);
    }

    private void Printer_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        try
        {
            if (e.PropertyName == nameof(PrinterModel.Status))
            {
                var printer = (PrinterModel)sender!;
                notificationService.UpdateStatus(printer);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}