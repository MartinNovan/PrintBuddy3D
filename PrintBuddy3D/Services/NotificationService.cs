using System;
using PrintBuddy3D.Models;
using PrintBuddy3D.ViewModels.Pages;

namespace PrintBuddy3D.Services;

public interface INotificationService
{
    void UpdateStatus(PrinterModel printer);
}

public class NotificationService(HomeViewModel home) : INotificationService
{
    public void UpdateStatus(PrinterModel printer) => home.UpdateStatus(printer);
}