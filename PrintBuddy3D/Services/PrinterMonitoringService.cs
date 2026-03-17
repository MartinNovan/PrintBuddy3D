using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Avalonia.Threading;
using PrintBuddy3D.Enums;
using PrintBuddy3D.Models;

namespace PrintBuddy3D.Services;

public interface IPrinterMonitoringService : IDisposable
{
    void Start(ObservableCollection<PrinterModel> printers);
    void Stop();
}

public class PrinterMonitoringService(IPrintersService printersService) : IPrinterMonitoringService
{
    private const int DispatchIntervalMs = 200;

    private ObservableCollection<PrinterModel>? _printers;
    private readonly CancellationTokenSource _cts = new();

    private readonly ConcurrentDictionary<Guid, bool> _activeChecks = new();

    public void Start(ObservableCollection<PrinterModel> printers)
    {
        _printers = printers;
        Task.Run(() => SchedulerLoopAsync(_cts.Token));
    }

    public void Stop() => _cts.Cancel();

    public void Dispose()
    {
        Stop();
        _cts.Dispose();
    }

    private async Task SchedulerLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            if (_printers == null) break;
            
            var toCheck = _printers
                .Where(p => p.ShouldUpdate && !_activeChecks.ContainsKey(p.Id))
                .OrderBy(p => p.LastUpdate)
                .ToList();

            if (toCheck.Count == 0)
            {
                await Task.Delay(DispatchIntervalMs, ct);
                continue;
            }

            foreach (var printer in toCheck)
            {
                if (ct.IsCancellationRequested) break;

                if (_activeChecks.ContainsKey(printer.Id))
                {
                    continue;
                }

                _ = CheckPrinterAsync(printer, ct);

                await Task.Delay(DispatchIntervalMs, ct);
            }
        }
    }
    private async Task CheckPrinterAsync(PrinterModel printer, CancellationToken ct)
    {
        if (!_activeChecks.TryAdd(printer.Id, true)) return; 
        try
        {
            var status = await printersService.GetPrinterStatusAsync(printer, ct);

            printer.LastUpdate = DateTime.Now;

            if (status != printer.Status)
            {
                await Dispatcher.UIThread.InvokeAsync(
                    () => printer.ChangeStatus(status),
                    DispatcherPriority.Background, ct);
            }
        }
        catch (OperationCanceledException)
        {
            //ignore
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Monitor] {printer.Name}: {ex.Message}");
        }
        finally
        {
            _activeChecks.TryRemove(printer.Id, out _);
        }
    }
}