using System;
using AvaloniaWebView;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PrintBuddy3D.Models;

namespace PrintBuddy3D.ViewModels.Pages;

public partial class PrinterControlViewModel : ObservableObject
{
    public PrinterModel Printer { get; }
    private readonly Action _goBack;

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
}