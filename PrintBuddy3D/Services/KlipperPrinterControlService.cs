using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using PrintBuddy3D.Enums;
using PrintBuddy3D.Models;

namespace PrintBuddy3D.Services;

public class KlipperPrinterControlService : IPrinterControlService
{
    private readonly PrinterModel _printer;
    private readonly HttpClient _httpClient;

    public KlipperPrinterControlService(PrinterModel printer)
    {
        _printer = printer;
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10)};
        if (string.IsNullOrEmpty(_printer.FullAddress) || string.IsNullOrEmpty(_printer.Address) || _printer.Prefix == null) return;
        _httpClient.BaseAddress = new Uri(_printer.FullAddress);
    }

    public async Task<PrinterEnums.Status> GetStatusAsync(CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(_printer.FullAddress) || string.IsNullOrEmpty(_printer.Address) || _printer.Prefix == null) return PrinterEnums.Status.Error; // show error to not confuse users about their config (if offline they would not suspect their config is bad)

        try
        {
            // Get state from Moonraker API using query
            var url = $"{_printer.FullAddress}/printer/objects/query?print_stats&webhooks";
            var response = await _httpClient.GetFromJsonAsync<JsonNode>(url, ct);

            if (response?["result"]?["status"] is not { } statusNode) 
                return PrinterEnums.Status.Error;

            // 1. Get the webhooks state
            string state = statusNode["webhooks"]?["state"]?.ToString().ToLower() ?? "unknown";
            
            // 2. Get the print state
            string printState = statusNode["print_stats"]?["state"]?.ToString().ToLower() ?? "";
            
            if (state == "shutdown") return PrinterEnums.Status.ShutDown;
            if (state == "startup") return PrinterEnums.Status.StartUp;
            if (state == "error") return PrinterEnums.Status.Error;
            
            if (state == "ready")
            {
                if (printState == "printing") return PrinterEnums.Status.Printing;
                if (printState == "paused") return PrinterEnums.Status.Paused;
                if (printState == "complete") return PrinterEnums.Status.Complete;
            }

            return PrinterEnums.Status.StandBy;
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Error getting klipper printer status for printer {_printer.Name}: " + ex.Message);
            return PrinterEnums.Status.Offline; // printer is most probably turned off
        }
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
    
    public async Task SendCommand(string command)
    {
        if (string.IsNullOrEmpty(_printer.FullAddress)) return ;

        try
        {
            // Moonraker API for sending G-Code
            // Example: http://klipper.local/printer/gcode/script?script=G28 So we just need the last piece and insert gcode
            var url = $"/printer/gcode/script?script={Uri.EscapeDataString(command)}";
            await _httpClient.PostAsync(url, null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending command: {ex.Message}");
        }
    }

    public void Move(string axis, double distance, int speed)
    {
        // G91 = Relative position (move by 10mm, not move to 10mm)
        // G1 {axis}{distance} F{speed} = the travel
        // G90 = Back to the absolute position
        var gcode = $"G91\nG1 {axis}{distance} F{speed}\nG90";
        _ = SendCommand(gcode);
    }

    public void Home(string axis = "all")
    {
        _ = SendCommand(axis.ToLower() == "all" ? "G28" : $"G28 {axis}");
    }

    //TODO: Make support for multi extruder printer
    public void SetTemperature(int temp, string type = "extruder") // type: extruder/heater_bed
    {
        string cmd;
        if (type == "heater_bed")
        {
            cmd = $"M140 S{temp}"; // Set bed temp
        }
        else
        {
            cmd = $"M104 S{temp}"; // Set extruder temp
        }
        _ = SendCommand(cmd);
    }

    public void DisableMotors()
    {
        _ = SendCommand("M84");
    }

    public void EmergencyStop()
    {
        _ = SendCommand("M112");
    }
    
    public async Task<List<ConsoleLogItem>> GetConsoleHistoryAsync()
    {
        if (string.IsNullOrEmpty(_printer.FullAddress)) return new List<ConsoleLogItem>();
    
        try
        {
            var url = $"{_printer.FullAddress}/server/gcode_store?count=100";
            var response = await _httpClient.GetFromJsonAsync<JsonNode>(url);
    
            if (response?["result"]?["gcode_store"] is JsonArray store)
            {
                var logs = new List<ConsoleLogItem>();
                foreach (var item in store)
                {
                    var rawMsg = item?["message"]?.ToString();
                    var time = item?["time"]?.GetValue<double>() ?? 0;

                    if (string.IsNullOrEmpty(rawMsg)) continue;
                    ConsoleLogType type = ConsoleLogType.Command;
                        
                    // Klipper logs:
                    // !! -> Error
                    // // -> Info
                    // no prefix -> Command
                        
                    if (rawMsg.StartsWith("!!"))
                    {
                        type = ConsoleLogType.Error;
                    }
                    else if (rawMsg.StartsWith("//"))
                    {
                        type = ConsoleLogType.Info;
                    }
                    else if (rawMsg.StartsWith("ok"))
                    {
                        type = ConsoleLogType.Success;
                    }
                    // We need to clean at the start of the line (trim function didnt delete / when it was like this \n//)
                    var lines = rawMsg.Split('\n');
                    for (int i = 0; i < lines.Length; i++)
                    {
                        lines[i] = lines[i].TrimStart('!', '/', ' ');
                    }
                    string cleanMsg = string.Join("\n", lines);
                    logs.Add(new ConsoleLogItem(cleanMsg, time, type));
                }
                return logs;
            }
        }
        catch (Exception)
        {
            // ignored
        }

        return new List<ConsoleLogItem>();
    }

}