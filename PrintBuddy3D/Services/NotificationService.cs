using System;
using PrintBuddy3D.Models;
using PrintBuddy3D.ViewModels.Pages;

namespace PrintBuddy3D.Services;

public interface INotificationService
{
    event Action<PrinterModel>? PrinterRegistered;
    event Action<PrinterModel>? StatusUpdated;
    
    void RegisterPrinter(PrinterModel printer);
    void UpdateStatus(PrinterModel printer);
}

public class NotificationService : INotificationService
{
    public event Action<PrinterModel>? PrinterRegistered;
    public event Action<PrinterModel>? StatusUpdated;
    public void RegisterPrinter(PrinterModel printer) 
        => PrinterRegistered?.Invoke(printer);
    public void UpdateStatus(PrinterModel printer) 
        => StatusUpdated?.Invoke(printer);
}