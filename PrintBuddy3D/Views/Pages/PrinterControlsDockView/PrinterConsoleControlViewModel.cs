using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading; // Důležité pro DispatcherTimer
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dock.Model.Mvvm.Controls;
using PrintBuddy3D.Services;

namespace PrintBuddy3D.Views.Pages.PrinterControlsDockView;

public partial class PrinterConsoleControlViewModel : Tool, IDisposable
{
    private readonly IPrinterControlService _printerControlService;
    private readonly DispatcherTimer _timer;
    private double _lastLogTime = 0; // Časová značka poslední přijaté zprávy

    [ObservableProperty] private string? _command;
    [ObservableProperty] private ObservableCollection<ConsoleLogItem>? _output = new();
    public PrinterConsoleControlViewModel(IPrinterControlService printerControlService)
    {
        _printerControlService = printerControlService;
        
        // Nastavení časovače pro stahování logů každou vteřinu
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += async (sender, e) => await FetchLogs();
        _timer.Start();
    }
    
    public void Dispose()
    {
        if (_timer.IsEnabled)
        {
            _timer.Stop();
        }
    }
    private async Task FetchLogs()
    {
        var logs = await _printerControlService.GetConsoleHistoryAsync();
        var newLogs = logs.Where(l => l.Time > _lastLogTime).ToList();
        if (newLogs.Any())
        {
            _lastLogTime = newLogs.Max(l => l.Time);
            foreach (var log in newLogs) 
            {
                Output?.Insert(0, log);
            }
        }
    }

    [RelayCommand]
    private void SeeHelp()
    {
        Command = "help";
        SendCommandToPrinter();
    }
    
    [RelayCommand]
    private void SendCommandToPrinter()
    {
        if (!string.IsNullOrEmpty(Command))
        {
            _printerControlService.SendCommand(Command);
        }
        Command = string.Empty;
    }
}