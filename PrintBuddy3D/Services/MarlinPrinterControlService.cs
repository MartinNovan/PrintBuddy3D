using System;
using PrintBuddy3D.Models;

namespace PrintBuddy3D.Services;

public class MarlinPrinterControlService(PrinterModel printer) : IPrinterControlService
{
    public void SendCommand(string command)
    {
        Console.WriteLine("Sending marlin a command: " + command + $" using port {printer.LastSerialPort} and baudrate {printer.BaudRate}");
    }

    public void Move()
    {
        throw new System.NotImplementedException();
    }

    public void Home()
    {
        throw new System.NotImplementedException();
    }

    public void SetTemperature(int temp)
    {
        throw new System.NotImplementedException();
    }
}