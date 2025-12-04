using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using PrintBuddy3D.Enums;
using PrintBuddy3D.Models;
using PrintBuddy3D.Services;

namespace PrintBuddy3D.ViewModels.Pages;

public partial class PrinterEditorViewModel : ObservableObject
{
    public PrinterModel EditingPrinter { get; }
    
    private readonly IPrintersService _printersService;
    private readonly Action _goBack;

    // ComboBox
    public PrinterEnums.Firmware[] FirmwareOptions { get; } = Enum.GetValues<PrinterEnums.Firmware>();
    public PrinterEnums.Prefix[] PrefixOptions { get; } = Enum.GetValues<PrinterEnums.Prefix>();
    public int[] BaudRateOptions { get; } = { 2400, 9600, 19200, 38400, 57600, 115200, 250000, 500000, 1000000 };
    
    public PrinterEditorViewModel(PrinterModel? printer, IPrintersService printersService, Action goBack)
    {
        EditingPrinter = printer ?? new PrinterModel();
        _printersService = printersService;
        _goBack = goBack;
        
        // Default values for printer
        if (printer == null)
        {
            EditingPrinter.Name = "New Printer";
            EditingPrinter.Firmware = PrinterEnums.Firmware.Klipper;
            EditingPrinter.Prefix = PrinterEnums.Prefix.Http;
            EditingPrinter.BaudRate = 115200;
        }
    }
    
    [RelayCommand]
    private void Back()
    {
        _goBack();
    }

    [RelayCommand]
    private async Task Save()
    {
        await _printersService.UpsertPrinterAsync(EditingPrinter);

        var listVm = App.Services.GetRequiredService<PrintersListViewModel>();
        if (!listVm.Printers.Contains(EditingPrinter))
        {
            listVm.Printers.Add(EditingPrinter);
        }
        
        _goBack();
    }
}