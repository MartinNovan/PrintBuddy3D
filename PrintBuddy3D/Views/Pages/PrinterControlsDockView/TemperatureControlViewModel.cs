
using Dock.Model.Mvvm.Controls;
using PrintBuddy3D.Models;
using PrintBuddy3D.Services;

namespace PrintBuddy3D.Views.Pages.PrinterControlsDockView;

public class TemperatureControlViewModel(
    IPrinterControlService contextPrinterControlService,
    PrinterModel contextPrinter) : Tool
{
    
}