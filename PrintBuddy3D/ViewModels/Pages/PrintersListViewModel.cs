using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PrintBuddy3D.Enums;
using PrintBuddy3D.Models;
using PrintBuddy3D.Services;
using SukiUI.Dialogs;

namespace PrintBuddy3D.ViewModels.Pages;

public partial class PrintersListViewModel : ObservableObject
{
    private readonly ISukiDialogManager _dialogManager;
    private readonly IPrintersService _printersService;
    private readonly IPrinterMonitoringService _printerMonitoringService;

    [ObservableProperty] private ObservableCollection<PrinterModel> _printers;
    [ObservableProperty] private object? _currentContent;
    
    public PrintersListViewModel(ISukiDialogManager dialogManager, IPrintersService printersService, IPrinterMonitoringService printerMonitoringService)
    {
        _dialogManager = dialogManager;
        _printersService = printersService;
        _printerMonitoringService = printerMonitoringService;
        CurrentContent = this;
        _ = LoadPrinters();
    }

    
    [RelayCommand]
    private void OpenPrinterControl(PrinterModel printer)
    {
        CurrentContent = new PrinterControlViewModel(printer, GoBack);
    }
    
    [RelayCommand]
    private void OpenPrinterEditor(PrinterModel? printer)
    {
        CurrentContent = new PrinterEditorViewModel(printer, _printersService, GoBack);
    }
    
    [RelayCommand]
    private void SshIntoPrinter(PrinterModel printer)
    {
        try
        {
            var dialog = _dialogManager.CreateDialog()
                .OfType(NotificationType.Error)
                .WithTitle("SSH Connection Error")
                .WithActionButton("Dismiss", _ => { }, true)
                .Dismiss().ByClickingBackground();
            if (String.IsNullOrEmpty(printer.Address) || printer.Firmware != PrinterEnums.Firmware.Klipper)
            {
                dialog.WithContent(
                        "SSH connection is only available for Klipper firmware printers with a valid address. Please check the printer settings.")
                    .TryShow();
                return;
            }

            if (String.IsNullOrEmpty(printer.HostUserName))
            {
                dialog.WithContent(
                        "SSH connection requires a valid host username. Please set the host username in the printer settings.")
                    .TryShow();
                return;
            }

            string sshCommand =
                $"ssh {printer.HostUserName}@{printer.Address.Replace("https://", "").Replace("http://", "")}";
            ProcessStartInfo psi = new ProcessStartInfo();
            if (OperatingSystem.IsWindows())
            {
                psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoExit -Command \"{sshCommand}\"",
                    UseShellExecute = false
                };
            }
            else if (OperatingSystem.IsLinux())
            {
                // gnome-terminal
                if (IsProgramInstalled("gnome-terminal"))
                {
                    psi.FileName = "gnome-terminal";
                    psi.Arguments = $"-- {sshCommand}";
                }
                // gnome-console
                else if (IsProgramInstalled("kgx"))
                {
                    psi.FileName = "kgx";
                    psi.Arguments = $"-e {sshCommand}";
                }
                // konsole
                else if (IsProgramInstalled("konsole"))
                {
                    psi.FileName = "konsole";
                    psi.Arguments = $"-e {sshCommand}";
                }
                // xfce-terminal
                else if (IsProgramInstalled("xfce4-terminal"))
                {
                    psi.FileName = "xfce4-terminal";
                    psi.Arguments = $"-e \"{sshCommand}\"";
                }
                // xterm
                else if (IsProgramInstalled("xterm"))
                {
                    psi.FileName = "xterm";
                    psi.Arguments = $"-e {sshCommand}";
                }
                // mate-terminal
                else if (IsProgramInstalled("mate-terminal"))
                {
                    psi.FileName = "mate-terminal";
                    psi.Arguments = $"-x {sshCommand}";
                }
                // kitty
                else if (IsProgramInstalled("kitty"))
                {
                    psi.FileName = "kitty";
                    psi.Arguments = $"-e {sshCommand}";
                }
                // alacritty
                else if (IsProgramInstalled("alacritty"))
                {
                    psi.FileName = "alacritty";
                    psi.Arguments = $"-e {sshCommand}";
                }
                else
                {
                    // Fallback: try x-terminal-emulator (Debian/Ubuntu system alias)
                    psi.FileName = "x-terminal-emulator";
                    psi.Arguments = $"-e {sshCommand}";
                }
            }

            Process.Start(psi);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"There was an issue opening a terminal: {ex.Message}");
        }
    }
    
    private bool IsProgramInstalled(string programName)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "/bin/sh",
                Arguments = $"-c \"command -v {programName}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
        
            var proc = Process.Start(psi);
            proc?.WaitForExit();
            return proc is { ExitCode: 0 };
        }
        catch
        {
            return false;
        }
    }
    
    [RelayCommand]
    private void RemovePrinter(PrinterModel printer)
    {
        if (Printers.Contains(printer))
        {
            _dialogManager.CreateDialog()
                .OfType(NotificationType.Warning)
                .WithTitle("Removing Printer")
                .WithContent($"Are you sure you want to remove the printer '{printer.Name}'?")
                .WithActionButton("Yes", _ =>
                {
                    _printersService.RemovePrinterAsync(printer);
                    Printers.Remove(printer);
                }, true).WithActionButton("No", _ => { }, true)
                .Dismiss().ByClickingBackground()
                .TryShow();
        }
    }

    private void GoBack()
    {
        CurrentContent = this; 
    }
    

    private async Task LoadPrinters()
    {
        Printers = await _printersService.GetPrintersAsync();
        _printerMonitoringService.Start(Printers);
    }
}