using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using PrintBuddy3D.Models;

namespace PrintBuddy3D.ViewModels.Pages;

public partial class PrinterEditorViewModel : ObservableObject
{
    public PrinterModel EditingPrinter { get; }
    private readonly Action _goBack;
    
    public PrinterEditorViewModel(PrinterModel? printer, Action goBack)
    {
        EditingPrinter = printer ?? new PrinterModel();
        _goBack = goBack;
    }
    
    [RelayCommand]
    private void Back()
    {
        _goBack();
    }

    [RelayCommand]
    private void Save()
    {
        // Validate the printer model before saving
        App.Services.GetRequiredService<PrintersListViewModel>().Printers.Add(EditingPrinter);
        _goBack();
    }
}