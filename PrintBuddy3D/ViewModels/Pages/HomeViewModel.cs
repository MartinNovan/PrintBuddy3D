using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using PrintBuddy3D.Enums;
using PrintBuddy3D.Models;
using PrintBuddy3D.Services;
using SukiUI.Toasts;

namespace PrintBuddy3D.ViewModels.Pages;

public partial class HomeViewModel : PageBase
{
    private readonly Dictionary<Guid, PrinterEnums.Status> _printerStates = new();

    [ObservableProperty] 
    private ObservableCollection<PrinterStatusSummary> _statusSummary = new();
    
    [ObservableProperty] 
    private ObservableCollection<string> _notificationMessages = new();

    private readonly ISukiToastManager _sukiToastManager;

    public HomeViewModel(ISukiToastManager toastManager, INotificationService notificationService) 
        : base("Home", MaterialIconKind.Home, 0)
    {
        _sukiToastManager = toastManager;
        notificationService.PrinterRegistered += RegisterPrinter;
        notificationService.StatusUpdated += UpdateStatus;
    }

    public void RegisterPrinter(PrinterModel printer)
    {
        _printerStates[printer.Id] = PrinterEnums.Status.Offline;
        RefreshSummary();
    }

    public void UpdateStatus(PrinterModel printer)
    {
        _printerStates[printer.Id] = printer.Status;
        RefreshSummary();

        if (!printer.IsInitialized) return;
        
        if (_notificationTemplates.TryGetValue(printer.Status, out var template))
        {
            if (printer.Status == PrinterEnums.Status.Offline && 
                printer.PreviousStatus == PrinterEnums.Status.None) return;

            var message = string.Format(template, printer.CurrentJob);
            NotificationMessages.Insert(0, $"{printer.Name}: {DateTime.Now}\n{message}");

            Dispatcher.UIThread.InvokeAsync(() =>
                _sukiToastManager.CreateToast()
                    .WithTitle(printer.Name ?? "Unnamed printer")
                    .WithContent(message)
                    .OfType(NotificationType.Information)
                    .Dismiss().ByClicking()
                    .Dismiss().After(TimeSpan.FromSeconds(10))
                    .Queue()
            );
        }
    }

    private void RefreshSummary()
    {
        var counts = _printerStates.Values
            .GroupBy(s => s)
            .ToDictionary(g => g.Key, g => g.Count());

        var toRemove = StatusSummary
            .Where(s => !counts.ContainsKey(s.Status) || counts[s.Status] == 0)
            .ToList();
        
        foreach (var item in toRemove) StatusSummary.Remove(item);

        foreach (var (status, count) in counts.Where(c => c.Value > 0))
        {
            var existing = StatusSummary.FirstOrDefault(s => s.Status == status);
            if (existing != null)
                existing.Count = count;
            else
                StatusSummary.Add(new PrinterStatusSummary(status, count));
        }
    }

    private readonly Dictionary<PrinterEnums.Status, string> _notificationTemplates = new()
    {
        { PrinterEnums.Status.StandBy,  "🟢 Printer is now online." },
        { PrinterEnums.Status.Offline,  "🔴 Printer went offline." },
        { PrinterEnums.Status.Complete, "✅ Printer finished printing {0}." },
        { PrinterEnums.Status.Printing, "🖨️ Printer started printing {0}." },
        { PrinterEnums.Status.Error,    "⚠️ Printer encountered an error." },
        { PrinterEnums.Status.Paused,   "⏸️ Print was paused." },
    };

    [RelayCommand]
    private void RemoveNotification(string message)
        => NotificationMessages.Remove(message);

    [RelayCommand]
    private void RemoveAllNotification()
        => NotificationMessages.Clear();
}

public partial class PrinterStatusSummary(PrinterEnums.Status status, int count) 
    : ObservableObject
{
    public PrinterEnums.Status Status { get; } = status;
    
    [ObservableProperty] 
    private int _count = count;
    
    public string Label => Status switch
    {
        PrinterEnums.Status.Offline  => "🔴 Offline",
        PrinterEnums.Status.StandBy  => "🟢 Stand by",
        PrinterEnums.Status.Printing => "🖨️ Printing",
        PrinterEnums.Status.Complete => "✅ Done",
        PrinterEnums.Status.Error    => "⚠️ Error",
        PrinterEnums.Status.Busy     => "⚙️ Busy",
        PrinterEnums.Status.Paused   => "⏸️ Paused",
        PrinterEnums.Status.StartUp  => "🔄 Starting",
        PrinterEnums.Status.ShutDown => "⛔ Shutdown",
        _ => Status.ToString()
    };
}