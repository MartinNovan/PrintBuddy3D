using System;
using PrintBuddy3D.Models;

namespace PrintBuddy3D.Services;

public class KlipperPrinterControlService(PrinterModel printer) : IPrinterControlService
{
    public void SendCommand(string command)
    {
        Console.WriteLine("Sending klipper a command: " + command + " to address: " + printer.FullAddress);
    }

    public void GetInfo()
    {
        Console.WriteLine("Printing klipper info");
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