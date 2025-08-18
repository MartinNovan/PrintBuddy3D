using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using PrintBuddy3D.Models;

namespace PrintBuddy3D.Services;

public interface IPrintersService
{
    Task<ObservableCollection<PrinterModel>> GetPrintersAsync();
}

public class PrintersService(IAppDataService appDataService) : IPrintersService
{
    private readonly SqliteConnection _dbConnection = appDataService.DbConnection;
    
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
                Status = "Online",
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
                Status = "Offline",
            },
            new()
            {
                Name = "Printer 2",
                Firmware = "Marlin",
                ConnectionType = "USB",
                SerialPort = "Com3",
                DbHash = 654321,
                Status = "Printing",
            },
            new()
            {
                Name = "Printer 2",
                Firmware = "Marlin",
                ConnectionType = "USB",
                SerialPort = "Com3",
                DbHash = 654321,
                Status = "Done",
            },
            new()
            {
                Name = "Printer 2",
                Firmware = "Marlin",
                ConnectionType = "USB",
                SerialPort = "Com3",
                DbHash = 654321,
                Status = "Idle",
            }
        };
        return Task.FromResult(printers);
    }
}