using System;
using System.Collections.Generic;
using PrintBuddy3D.Enums;
using PrintBuddy3D.Models;

namespace PrintBuddy3D.Services;

public interface IPrinterControlServiceFactory
{
    IPrinterControlService Create(PrinterModel printer);
    public void Invalidate(PrinterModel printer);
    public void Dispose();
}

public class PrinterControlServiceFactory : IPrinterControlServiceFactory
{
    private readonly Dictionary<Guid, IPrinterControlService> _cache = new();

    public IPrinterControlService Create(PrinterModel printer)
    {
        if (_cache.TryGetValue(printer.Id, out var value))
            return value;

        var service = printer.Firmware switch
        {
            PrinterEnums.Firmware.Marlin  => (IPrinterControlService)new MarlinPrinterControlService(printer),
            PrinterEnums.Firmware.Klipper => new KlipperPrinterControlService(printer),
            _ => throw new NotSupportedException($"Firmware {printer.Firmware} is not supported")
        };

        _cache[printer.Id] = service;
        return service;
    }

    public void Invalidate(PrinterModel printer)
    {
        if (_cache.TryGetValue(printer.Id, out var service))
        {
            service.Dispose();
            _cache.Remove(printer.Id);
        }
    }

    public void Dispose()
    {
        foreach (var service in _cache.Values)
            service.Dispose();
        _cache.Clear();
    }
}