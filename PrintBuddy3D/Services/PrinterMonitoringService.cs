using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
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
    private const int DispatchIntervalMs = 200; // Some small pause between loops of checks
    private readonly SemaphoreSlim _throttle = new(50, 50); // Max 50 printers to check parallel

    private ObservableCollection<PrinterModel>? _printers;
    private readonly CancellationTokenSource _cts = new();

    private readonly ConcurrentDictionary<Guid, bool> _activeChecks = new(); // Dictionary for printers thats currently checking
    private readonly ConcurrentQueue<(PrinterModel printer, PrinterEnums.Status status)> _pendingUpdates = new(); // Queue for UI batch updates

    public void Start(ObservableCollection<PrinterModel> printers)
    {
        _printers = printers;
        _ = Task.Delay(TimeSpan.FromSeconds(2), _cts.Token)// Wait 2s for UI to load (i hope this helps at least a little bit)
            .ContinueWith(_ => SchedulerLoopAsync(_cts.Token), _cts.Token);
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
            if (_printers == null)
            {
                await Task.Delay(DispatchIntervalMs, ct); // wait a while if no printers is in app
                continue;
            }

            var snapshot = _printers.ToList(); // Snapshot for thread safe operation
            
            var toCheck = snapshot
                .Where(p => p.ShouldUpdate && !_activeChecks.ContainsKey(p.Id))
                .OrderBy(p => p.LastUpdate)
                .ToList(); // Found printers that should update and order the by the LastUpdate property

            if (toCheck.Count == 0)
            {
                await Task.Delay(DispatchIntervalMs, ct); // wait a while if no printers is there to check
                continue;
            }

            foreach (var printer in toCheck)
            {
                if (ct.IsCancellationRequested) break;

                if (_activeChecks.ContainsKey(printer.Id))
                {
                    continue; // if printer is already checking skip it
                }

                _ = CheckPrinterAsync(printer, ct); // Check the printer
            }
            
            await Task.Delay(DispatchIntervalMs, ct); // wait a while before next loop

            // Make one batch of updates to UI instead of update per printer 
            // I really hate optimizing
            if (!_pendingUpdates.IsEmpty)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    while (_pendingUpdates.TryDequeue(out var update))
                        update.printer.ChangeStatus(update.status);
                }, DispatcherPriority.Background);
            }
        }
    }
    
    private async Task CheckPrinterAsync(PrinterModel printer, CancellationToken ct)
    {
        if (!_activeChecks.TryAdd(printer.Id, true)) return; // if printer is already in active checking skip this call
        
        await _throttle.WaitAsync(ct); // Add this printer to semaphore to check only 50 printer max at once
        try
        {
            var status = await printersService.GetPrinterStatusAsync(printer, ct);
            printer.LastUpdate = DateTime.Now;

            if (status != printer.Status)
                _pendingUpdates.Enqueue((printer, status)); // Add this printer to pending updates to UI to update it once per loop
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            Console.WriteLine($"[Monitor] {printer.Name}: {ex.Message}");
        }
        finally
        {
            _throttle.Release();
            _activeChecks.TryRemove(printer.Id, out _);
        }
    }
}