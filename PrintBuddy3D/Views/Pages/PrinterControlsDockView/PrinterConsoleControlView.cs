using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dock.Model.Mvvm.Controls;
using PrintBuddy3D.Services;

namespace PrintBuddy3D.Views.Pages.PrinterControlsDockView;

public partial class PrinterConsoleControlViewModel(IPrinterControlService printerControlService) : Tool
{
    [ObservableProperty] private string? _command;
    [ObservableProperty] private ObservableCollection<string>? _output = new();

    [RelayCommand]
    private void SendCommandToPrinter()
    {
        if (!string.IsNullOrEmpty(Command))
        {
            printerControlService.SendCommand(Command);
            Output?.Insert(0, $"-> [INFO] Send: {Command}");
        }

        Command = string.Empty;
    }
}