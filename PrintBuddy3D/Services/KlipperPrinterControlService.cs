using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
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
        _httpClient = new HttpClient();
        if (!string.IsNullOrEmpty(_printer.FullAddress))
        {
            _httpClient.BaseAddress = new Uri(_printer.FullAddress);
        }
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
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
            Console.WriteLine($"Chyba při odesílání příkazu: {ex.Message}");
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