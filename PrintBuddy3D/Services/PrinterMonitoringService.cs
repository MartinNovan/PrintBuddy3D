using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
    private readonly Dictionary<Guid, CancellationTokenSource> _loops = new();
    private ObservableCollection<PrinterModel>? _printers;
    private readonly SemaphoreSlim _httpSemaphore = new(20);

    public void Start(ObservableCollection<PrinterModel> printers)
    {
        _printers = printers;
        _printers.CollectionChanged += OnCollectionChanged;
        foreach (var printer in printers)
            StartLoop(printer);
    }

    public void Stop()
    {
        if (_printers != null)
            _printers.CollectionChanged -= OnCollectionChanged;
        foreach (var cts in _loops.Values)
            cts.Cancel();
        _loops.Clear();
    }

    public void Dispose()
    {
        Stop();
        _httpSemaphore.Dispose();
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
            foreach (PrinterModel p in e.NewItems) StartLoop(p);
        if (e.OldItems != null)
            foreach (PrinterModel p in e.OldItems) StopLoop(p);
    }

    private void StartLoop(PrinterModel printer)
    {
        var cts = new CancellationTokenSource();
        _loops[printer.Id] = cts;
        Task.Run(() => PrinterLoopAsync(printer, cts.Token));
    }

    private void StopLoop(PrinterModel printer)
    {
        if (_loops.TryGetValue(printer.Id, out var cts))
        {
            cts.Cancel();
            cts.Dispose();
            _loops.Remove(printer.Id);
        }
    }

    private async Task PrinterLoopAsync(PrinterModel printer, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                PrinterEnums.Status status;

                await _httpSemaphore.WaitAsync(ct);
                try
                {
                    status = await printersService.GetPrinterStatusAsync(printer, ct);
                }
                finally
                {
                    _httpSemaphore.Release();
                }
                if (status != printer.Status)
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        printer.ChangeStatus(status);
                        printer.LastUpdate = DateTime.Now;
                    }, DispatcherPriority.Background, ct);
                }
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                Console.WriteLine($"[Monitor] {printer.Name}: {ex.Message}");
            }

            try
            {
                await Task.Delay(printer.RefreshInterval, ct);
            }
            catch (OperationCanceledException) { break; }
        }
    }
}