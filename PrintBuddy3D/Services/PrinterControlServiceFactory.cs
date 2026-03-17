using System;
using System.Collections.Concurrent;
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
    private readonly ConcurrentDictionary<Guid, IPrinterControlService> _cache = new();

    public IPrinterControlService Create(PrinterModel printer)
    {
        return _cache.GetOrAdd(printer.Id, _ => printer.Firmware switch
        {
            PrinterEnums.Firmware.Marlin  => new MarlinPrinterControlService(printer),
            PrinterEnums.Firmware.Klipper => new KlipperPrinterControlService(printer),
            _ => throw new NotSupportedException($"Firmware {printer.Firmware} is not supported")
        });
    }

    public void Invalidate(PrinterModel printer)
    {
        if (_cache.TryRemove(printer.Id, out var service))
        {
            service.Dispose();
        }
    }

    public void Dispose()
    {
        foreach (var service in _cache.Values)
            service.Dispose();
        _cache.Clear();
    }
}