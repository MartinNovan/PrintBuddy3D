using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PrintBuddy3D.Models;
using PrintBuddy3D.ViewModels.Pages;

namespace PrintBuddy3D.Services;

public interface IPrinterMonitoringService
{
    void Start(ObservableCollection<PrinterModel> printers);
    void Stop(); // mby will use stop somewhere in debug setting or something like that
}

public class PrinterMonitoringService(IPrintersService printersService) : IPrinterMonitoringService
{
    private ObservableCollection<PrinterModel>? _printers;
    private readonly SemaphoreSlim _semaphore = new(10); // max 10 update at the same time 
    private bool _running;

    public void Start(ObservableCollection<PrinterModel> printers)
    {
        _printers = printers;
        _running = true;
        Task.Run(UpdateLoopAsync);
    }

    public void Stop()
    {
        _running = false;
    }

    private async Task UpdateLoopAsync()
    {
        while (_running)
        {
            if (_printers == null || _printers.Count == 0)
            {
                await Task.Delay(500);
                continue;
            }

            var tasks = new List<Task>();

            foreach (var printer in _printers.Where(p => p.ShouldUpdate))
            {
                tasks.Add(UpdatePrinterSafeAsync(printer));
            }

            // start when all ready
            if (tasks.Count > 0)
                await Task.WhenAll(tasks);

            await Task.Delay(500);
        }
    }

    private async Task UpdatePrinterSafeAsync(PrinterModel printer)
    {
        await _semaphore.WaitAsync(); // limit paralelism 
        try
        {
            await UpdatePrinterAsync(printer);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task UpdatePrinterAsync(PrinterModel printer)
    {
        try
        {
            printer.UpdateLock = true;
            var status = await printersService.GetPrinterStatusAsync();
            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                printer.ChangeStatus(status);
                printer.LastUpdate = DateTime.Now;
            });
        }
        finally
        {
            printer.UpdateLock = false;
        }
    }

}
