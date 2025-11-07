using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PrintBuddy3D.Enums;
using PrintBuddy3D.Models;
using SukiUI.Toasts;

namespace PrintBuddy3D.ViewModels.Pages;

public partial class HomeViewModel : ObservableObject
{
    [ObservableProperty] private int _offlinePrintersCount;
    [ObservableProperty] private int _standByPrintersCount;
    [ObservableProperty] private int _printingPrintersCount;
    [ObservableProperty] private int _donePrintersCount;
    [ObservableProperty] private ObservableCollection<string> _notificationMessages = new();

    private readonly ISukiToastManager _sukiToastManager;

    public HomeViewModel(ISukiToastManager toastManager)
    {
        _sukiToastManager = toastManager;
    }

    [RelayCommand]
    private void RemoveNotification(string message)
    {
        if (NotificationMessages.Contains(message))
        {
            NotificationMessages.Remove(message);
        }
    }
    
    [RelayCommand]
    private void RemoveAllNotification()
    {
        NotificationMessages.Clear();
    }
    
    private readonly Dictionary<PrinterEnums.Status, string> _statusMessages = new()
    {
        { PrinterEnums.Status.StandBy, "🟢 Printer is now online." },
        { PrinterEnums.Status.Offline, "🔴 Printer went offline." },
        { PrinterEnums.Status.Complete, "✅ Printer finished printing {0}." },
        { PrinterEnums.Status.Printing, "🖨️ Printer is printing {0}." }
    };

    public void UpdateStatus(PrinterModel printer)
    {
        if (_statusMessages.TryGetValue(printer.Status, out var template))
        {
            switch (printer.Status)
            {
                case PrinterEnums.Status.StandBy:
                    StandByPrintersCount++;
                    break;
                case PrinterEnums.Status.Offline:
                    OfflinePrintersCount++;
                    break;
                case PrinterEnums.Status.Printing:
                    PrintingPrintersCount++;
                    break;
                case PrinterEnums.Status.Complete:
                    DonePrintersCount++;
                    break;
            }

            if (_statusMessages.TryGetValue(printer.PreviousStatus, out _))
            {
                switch (printer.PreviousStatus)
                {
                    case PrinterEnums.Status.StandBy:
                        StandByPrintersCount--;
                        break;
                    case PrinterEnums.Status.Offline:
                        OfflinePrintersCount--;
                        break;
                    case PrinterEnums.Status.Printing:
                        PrintingPrintersCount--;
                        break;
                    case PrinterEnums.Status.Complete:
                        DonePrintersCount--;
                        break;
                }
            }

            if (printer is { Status: PrinterEnums.Status.Offline, PreviousStatus: PrinterEnums.Status.None })
                return;

            string message = string.Format(template, printer.CurrentJob);
            NotificationMessages.Insert(0, printer.Name + $": {DateTime.Now}\n" + message);

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
}