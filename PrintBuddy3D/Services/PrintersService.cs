using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
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
    void Dispose();
}
public record ConsoleLogItem(string Message, double Time, ConsoleLogType type);
public class PrintersService(IAppDataService appDataService, INotificationService notificationService) : IPrintersService
{
    private readonly string _connectionString = appDataService.ConnectionString;
    private readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(2) };

    
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
        await using var connection = new SqliteConnection(_connectionString); 
        await connection.OpenAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Printers WHERE Id = $id";
        command.Parameters.AddWithValue("$id", printer.Id);
        await command.ExecuteNonQueryAsync(ct);
    }

    public async Task<PrinterEnums.Status> GetPrinterStatusAsync(PrinterModel printer, CancellationToken ct = default)
    {
        return printer.Firmware switch
        {
            PrinterEnums.Firmware.Klipper => await CheckKlipperStatus(printer, ct),
            // TODO: Implement status for marlin
            PrinterEnums.Firmware.Marlin when !string.IsNullOrEmpty(printer.LastSerialPort) => PrinterEnums.Status.StandBy,
            _ => PrinterEnums.Status.Offline
        };
    }

    private async Task<PrinterEnums.Status> CheckKlipperStatus(PrinterModel printer, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(printer.FullAddress)) return PrinterEnums.Status.Offline;

        try
        {
            // Get state from Moonraker API using query
            var url = $"{printer.FullAddress}/printer/objects/query?print_stats&webhooks";
            var response = await _httpClient.GetFromJsonAsync<JsonNode>(url, ct);

            if (response?["result"]?["status"] is not JsonNode statusNode) 
                return PrinterEnums.Status.Error;

            // 1. Get the webhooks state
            string state = statusNode["webhooks"]?["state"]?.ToString()?.ToLower() ?? "unknown";
            
            // 2. Get the print state
            string printState = statusNode["print_stats"]?["state"]?.ToString()?.ToLower() ?? "";
            
            if (state == "shutdown") return PrinterEnums.Status.ShutDown;
            if (state == "startup") return PrinterEnums.Status.StartUp;
            if (state == "error") return PrinterEnums.Status.Error;
            
            if (state == "ready")
            {
                if (printState == "printing") return PrinterEnums.Status.Printing;
                if (printState == "paused") return PrinterEnums.Status.Busy; // take Paused as Busy
                if (printState == "complete") return PrinterEnums.Status.Complete;
            }

            return PrinterEnums.Status.StandBy;
        }
        catch
        {
            return PrinterEnums.Status.Offline;
        }
    }

}