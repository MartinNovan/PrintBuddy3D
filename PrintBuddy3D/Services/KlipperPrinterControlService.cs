using System;
using System.Net.Http;
using System.Threading.Tasks;
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

    public async void SendCommand(string command)
    {
        if (string.IsNullOrEmpty(_printer.FullAddress)) return;

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
        SendCommand(gcode);
    }

    public void Home(string axis = "all")
    {
        SendCommand(axis.ToLower() == "all" ? "G28" : $"G28 {axis}");
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
        SendCommand(cmd);
    }

    public void DisableMotors()
    {
        SendCommand("M84");
    }
}