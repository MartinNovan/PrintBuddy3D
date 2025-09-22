using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PrintBuddy3D.Models;
using SukiUI.Toasts;

namespace PrintBuddy3D.ViewModels.Pages;

public partial class HomeViewModel : ObservableObject
{
    [ObservableProperty] private int _onlinePrintersCount;
    [ObservableProperty] private int _offlinePrintersCount;
    [ObservableProperty] private int _printingPrintersCount;
    [ObservableProperty] private int _donePrintersCount;
    [ObservableProperty] private int _idlePrintersCount;
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
    
    private readonly Dictionary<string, string> _statusMessages = new()
    {
        { "Online", "🟢 Printer is now online." },
        { "Offline", "🔴 Printer went offline." },
        { "Done", "✅ Printer finished printing {0}." },
        { "Printing", "🖨️ Printer is printing {0}." }
    };

    public void UpdateStatus(PrinterModel printer)
    {
        if (_statusMessages.TryGetValue(printer.Status ?? "", out var template))
        {
            switch (printer.Status)
            {
                case "Online":
                    OnlinePrintersCount += 1;
                    break;
                case "Offline":
                    OfflinePrintersCount += 1;
                    break;
                case "Printing":
                    PrintingPrintersCount += 1;
                    break;
                case "Done":
                    DonePrintersCount += 1;
                    break;
                case "Idle":
                    IdlePrintersCount += 1;
                    break;
            }
            if (printer.PreviousStatus != null && _statusMessages.TryGetValue(printer.PreviousStatus, out _))
            {
                switch (printer.PreviousStatus)
                {
                    case "Online":
                        OnlinePrintersCount -= 1;
                        break;
                    case "Offline":
                        OfflinePrintersCount -= 1;
                        break;
                    case "Printing":
                        PrintingPrintersCount -= 1;
                        break;
                    case "Done":
                        DonePrintersCount -= 1;
                        break;
                    case "Idle":
                        IdlePrintersCount -= 1;
                        break;
                }
            }
            if (printer is { Status: "Offline", PreviousStatus: "None" }) return; //to avoid showing a notification when the printer is offline from the start of the app
            string message = string.Format(template, printer.CurrentJob);
            NotificationMessages.Insert(0,message);
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