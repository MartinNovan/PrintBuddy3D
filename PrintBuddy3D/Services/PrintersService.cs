using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using PrintBuddy3D.Enums;
using PrintBuddy3D.Models;

namespace PrintBuddy3D.Services;

public interface IPrintersService : IDisposable
{
    Task<ObservableCollection<PrinterModel>> GetPrintersAsync(CancellationToken ct = default);
    Task UpsertPrinterAsync(PrinterModel printer, CancellationToken ct = default);
    Task RemovePrinterAsync(PrinterModel printer, CancellationToken ct = default);
    Task<PrinterEnums.Status> GetPrinterStatusAsync(PrinterModel printer, CancellationToken ct = default);
}

public interface IPrinterControlService
{
    Task SendCommand(string command);
    void Move(string axis, double distance, int speed);
    void Home(string axis);
    void SetTemperature(int temp, string type = "extruder");
    void DisableMotors();
    void EmergencyStop();
    
    Task<List<ConsoleLogItem>> GetConsoleHistoryAsync();
    Task<PrinterEnums.Status> GetStatusAsync(CancellationToken ct = default);

    void Dispose();
}
public record ConsoleLogItem(string Message, double Time, ConsoleLogType Type);
public class PrintersService(IAppDataService appDataService, INotificationService notificationService, IPrinterControlServiceFactory printerControlServiceFactory) : IPrintersService
{
    private readonly string _connectionString = appDataService.ConnectionString;

    public void Dispose() { }
    public async Task<ObservableCollection<PrinterModel>> GetPrintersAsync(CancellationToken ct = default)
    {
        var printers = new ObservableCollection<PrinterModel>();
        await using var connection = new SqliteConnection(_connectionString); 
        await connection.OpenAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Printers";

        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var printer = new PrinterModel
            {
                Id = reader.GetGuid(reader.GetOrdinal("Id")),
                DbHash = reader.GetInt32(reader.GetOrdinal("Hash")),
                Name = reader.IsDBNull(reader.GetOrdinal("Name")) ? null : reader.GetString(reader.GetOrdinal("Name")),
                Firmware = (PrinterEnums.Firmware)reader.GetInt32(reader.GetOrdinal("Firmware")),
                Prefix = (PrinterEnums.Prefix)reader.GetInt32(reader.GetOrdinal("Prefix")),
                Address = reader.IsDBNull(reader.GetOrdinal("Address")) ? null : reader.GetString(reader.GetOrdinal("Address")),
                HostUserName = reader.IsDBNull(reader.GetOrdinal("HostUserName")) ? null : reader.GetString(reader.GetOrdinal("HostUserName")),
                LastSerialPort = reader.IsDBNull(reader.GetOrdinal("LastSerialPort")) ? null : reader.GetString(reader.GetOrdinal("LastSerialPort")),
                BaudRate = reader.GetInt32(reader.GetOrdinal("BaudRate")),
                SerialNumber = reader.IsDBNull(reader.GetOrdinal("SerialNumber")) ? null : reader.GetString(reader.GetOrdinal("SerialNumber")),
                ImagePath = reader.IsDBNull(reader.GetOrdinal("ImagePath")) ? null : reader.GetString(reader.GetOrdinal("ImagePath"))
            };
            
            printer.PropertyChanged += async (s, e) =>
            {
                if (printer.Hash != printer.DbHash)
                {
                    if (e.PropertyName is 
                        nameof(PrinterModel.LastSerialPort) or
                        nameof(PrinterModel.BaudRate) or
                        nameof(PrinterModel.Address) or
                        nameof(PrinterModel.Prefix) or
                        nameof(PrinterModel.Firmware))
                    {
                        printerControlServiceFactory.Invalidate(printer);
                    }
                    
                    await UpsertPrinterAsync(printer, CancellationToken.None);
                    printer.DbHash = printer.Hash;
                }
                if (e.PropertyName == nameof(PrinterModel.Status))
                {
                    notificationService.UpdateStatus(printer);
                }
            };
            printers.Add(printer);
        }

        return printers;
    }

    public async Task UpsertPrinterAsync(PrinterModel printer, CancellationToken ct = default)
    {
        await using var connection = new SqliteConnection(_connectionString); 
        await connection.OpenAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Printers (Id, Hash, Name, Firmware, Prefix, Address, HostUserName, LastSerialPort, BaudRate, SerialNumber, ImagePath)
            VALUES ($id, $hash, $name, $firmware, $prefix, $address, $hostUser, $port, $baud, $serial, $image)
            ON CONFLICT(Id) DO UPDATE SET
                Hash = excluded.Hash,
                Name = excluded.Name,
                Firmware = excluded.Firmware,
                Prefix = excluded.Prefix,
                Address = excluded.Address,
                HostUserName = excluded.HostUserName,
                LastSerialPort = excluded.LastSerialPort,
                BaudRate = excluded.BaudRate,
                SerialNumber = excluded.SerialNumber,
                ImagePath = excluded.ImagePath;";

        command.Parameters.AddWithValue("$id", printer.Id);
        command.Parameters.AddWithValue("$hash", printer.Hash);
        command.Parameters.AddWithValue("$name", printer.Name ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$firmware", (int)(printer.Firmware ?? 0));
        command.Parameters.AddWithValue("$prefix", (int)(printer.Prefix ?? 0));
        command.Parameters.AddWithValue("$address", printer.Address ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$hostUser", printer.HostUserName ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$port", printer.LastSerialPort ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$baud", printer.BaudRate);
        command.Parameters.AddWithValue("$serial", printer.SerialNumber ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$image", printer.ImagePath ?? (object)DBNull.Value);

        await command.ExecuteNonQueryAsync(ct);
    }

    public async Task RemovePrinterAsync(PrinterModel printer, CancellationToken ct = default)
    {
        printerControlServiceFactory.Invalidate(printer); // removed cached service
        await using var connection = new SqliteConnection(_connectionString); 
        await connection.OpenAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Printers WHERE Id = $id";
        command.Parameters.AddWithValue("$id", printer.Id);
        await command.ExecuteNonQueryAsync(ct);
    }

    public async Task<PrinterEnums.Status> GetPrinterStatusAsync(PrinterModel printer, CancellationToken ct = default)
    {
        var service = printerControlServiceFactory.Create(printer);
        return await service.GetStatusAsync(ct);
    }
}