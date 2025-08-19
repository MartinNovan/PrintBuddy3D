using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using PrintBuddy3D.Models;

namespace PrintBuddy3D.Services;

public interface IPrintersService
{
    Task<ObservableCollection<PrinterModel>> GetPrintersAsync();
}

public class PrintersService(IAppDataService appDataService) : IPrintersService
{
    private readonly SqliteConnection _dbConnection = appDataService.DbConnection;
    private readonly INotificationService _notificationService = App.Services.GetRequiredService<INotificationService>();
    
    public Task<ObservableCollection<PrinterModel>> GetPrintersAsync()
    {
        var printers = new ObservableCollection<PrinterModel>()
        {
            new()
            {
                Name = "Printer 1",
                Firmware = "Klipper",
                ConnectionType = "WiFi",
                DbHash = 123456,
                Address = "http://klipper.local",
                Prefix = "http://",
                HostUserName = "klipper",
            },
            new()
            {
                Name = "Printer 1",
                Firmware = "Klipper",
                ConnectionType = "WiFi",
                DbHash = 123456,
                Prefix = "http://",
                Address = "http://10.0.0.88",
                HostUserName = "klipper",
            },
            new()
            {
                Name = "Printer 2",
                Firmware = "Marlin",
                ConnectionType = "USB",
                SerialPort = "Com3",
                DbHash = 654321,
            },
            new()
            {
                Name = "Printer 2",
                Firmware = "Marlin",
                ConnectionType = "USB",
                SerialPort = "Com3",
                DbHash = 654321,
            },
            new()
            {
                Name = "Printer 2",
                Firmware = "Marlin",
                ConnectionType = "USB",
                SerialPort = "Com3",
                DbHash = 654321,
            }
        };
        foreach (var printer in printers)
        {
            printer.PropertyChanged += Printer_PropertyChanged;
        }
        // Simulate random status updates at startup (will be replaced with real data fetching from the web)
        foreach (var printer in printers)
        {
            var status = Random.Shared.Next(0, 4);
            switch (status)
            {
                case 0:
                    printer.Status = "Online";
                    break;
                case 1:
                    printer.Status = "Offline";
                    break;
                case 2:
                    printer.Status = "Printing";
                    break;
                case 3:
                    printer.Status = "Done";
                    break;
                case 4:
                    printer.Status = "Idle";
                    break;
            }
        }
        return Task.FromResult(printers);
    }
    private void Printer_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        try
        {
            if (e.PropertyName == nameof(PrinterModel.Status))
            {
                var printer = (PrinterModel)sender!;
                _notificationService.UpdateStatus(printer);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}