using System;
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
        throw new System.NotImplementedException();
    }

    public void Home(string axis)
    {
        throw new NotImplementedException();
    }

    public void SetTemperature(int temp, string type = "extruder")
    {
        throw new NotImplementedException();
    }

    public void DisableMotors()
    {
        throw new NotImplementedException();
    }

}