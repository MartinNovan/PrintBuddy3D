using System;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PrintBuddy3D.Models;

namespace PrintBuddy3D.ViewModels.Pages;

public partial class PrinterControlViewModel : ObservableObject
{
    public PrinterModel Printer { get; }
    private readonly Action _goBack;
    
    [ObservableProperty] private bool _isWebModeEnabled;
    public PrinterControlViewModel(PrinterModel printer, Action goBack)
    {
        Printer = printer;
        _goBack = goBack;
    }
    
    [RelayCommand]
    private void Back()
    {
        _goBack();
    }

    [RelayCommand]
    private void SwitchModes()
    {
        //TODO: find suitable webview for linux or find other solution to open web interface in app
        if (OperatingSystem.IsLinux())
        {
            try
            {
                using var process = new Process();
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = Printer.Address,
                    UseShellExecute = true
                };
                process.Start();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error when opening URL: {ex.Message}");
            }
        }
        IsWebModeEnabled = !IsWebModeEnabled;
    }
}