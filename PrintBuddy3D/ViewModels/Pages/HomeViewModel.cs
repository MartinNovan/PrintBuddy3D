using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using Material.Icons.Avalonia;
using Microsoft.Extensions.DependencyInjection;
using PrintBuddy3D.Models;
using SukiUI.Enums;
using SukiUI.Toasts;

namespace PrintBuddy3D.ViewModels.Pages;

public partial class HomeViewModel : ViewModelBase
{
    [ObservableProperty] private int _onlinePrintersCount;
    [ObservableProperty] private int _offlinePrintersCount;
    [ObservableProperty] private int _printingPrintersCount;
    [ObservableProperty] private int _donePrintersCount;
    [ObservableProperty] private int _idlePrintersCount;
    [ObservableProperty] private ObservableCollection<string> _notificationMessages = new();

    private readonly ISukiToastManager _sukiToastManager;
    private readonly PrintersListViewModel _printersVm;
    public HomeViewModel(ISukiToastManager toastManager)
    {
        _sukiToastManager = toastManager;
        _printersVm = App.Services.GetRequiredService<PrintersListViewModel>();
        RefreshPrintersCount();
    }

    [RelayCommand]
    private void RemoveNotification(string message)
    {
        if (NotificationMessages.Contains(message))
        {
            NotificationMessages.Remove(message);
        }
    }
    
    public void RefreshPrintersCount()
    {
        OnlinePrintersCount = _printersVm.Printers?.Count(p => p.Status == "Online") ?? 0;
        OfflinePrintersCount = _printersVm.Printers?.Count(p => p.Status == "Offline") ?? 0;
        PrintingPrintersCount = _printersVm.Printers?.Count(p => p.Status == "Printing") ?? 0;
        DonePrintersCount = _printersVm.Printers?.Count(p => p.Status == "Done") ?? 0;
        IdlePrintersCount = _printersVm.Printers?.Count(p => p.Status == "Idle") ?? 0;
    }
    private readonly Dictionary<string, string> _statusMessages = new()
    {
        { "Online", "🟢 Printer is now online." },
        { "Offline", "🔴 Printer went offline." },
        { "Done", "✅ Printer finished printing {0}." },
        { "Printing", "🖨️ Printer started printing {0}." }
    };

    public void RecieveNotification(PrinterModel printer)
    {
        if (_statusMessages.TryGetValue(printer.Status ?? "", out var template))
        {
            string message = string.Format(template, printer.CurrentJob);
            NotificationMessages.Insert(0,message);
            _sukiToastManager.CreateToast()
                .WithTitle(printer.Name ?? "Unnamed printer")
                .WithContent(message)
                .OfType(NotificationType.Information)
                .Dismiss().ByClicking()
                .Dismiss().After(TimeSpan.FromSeconds(10))
                .Queue();
        }
    }
    
}