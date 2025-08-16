using System;
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
        IsWebModeEnabled = !IsWebModeEnabled;
    }
}