using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PrintBuddy3D.Models;

namespace PrintBuddy3D.Services;

public class MarlinPrinterControlService(PrinterModel printer) : IPrinterControlService
{
    public void SendCommand(string command)
    {
        Console.WriteLine("Sending marlin a command: " + command + $" using port {printer.LastSerialPort} and baudrate {printer.BaudRate}");
    }

    public void Move(string axis, double distance, int speed)
    {
        // G91 = Relative position (move by 10mm, not move to 10mm)
        // G1 {axis}{distance} F{speed} = the travel
        // G90 = Back to the absolute position
        var gcode = $"G91\nG1 {axis}{distance} F{speed}\nG90";
        SendCommand(gcode);
    }

    public void Home(string axis)
    {
        SendCommand(axis.ToLower() == "all" ? "G28" : $"G28 {axis}");
    }

    public void SetTemperature(int temp, string type = "extruder")
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

    public void EmergencyStop()
    {
        SendCommand("M999");
    }

    public Task<List<ConsoleLogItem>> GetConsoleHistoryAsync()
    {
        return Task.FromResult(new List<ConsoleLogItem>());
    }
} 