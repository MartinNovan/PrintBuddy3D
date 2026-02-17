using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dock.Model.Mvvm.Controls;
using PrintBuddy3D.Models;
using PrintBuddy3D.Services;

namespace PrintBuddy3D.Views.Pages.PrinterControlsDockView;

public partial class TemperatureControlViewModel(
    IPrinterControlService contextPrinterControlService,
    PrinterModel contextPrinter)
    : Tool
{
    private readonly IPrinterControlService? printerControlService = contextPrinterControlService;
    public PrinterModel Printer { get; } = contextPrinter;

    [ObservableProperty] private int _targetNozzleTemp = 0;
    [ObservableProperty] private int _targetBedTemp = 0;

    // Preset list
    // TODO Make this editable (mby take it from the rpi) idk we will see
    public ObservableCollection<TemperaturePreset> Presets { get; } = new()
    {
        new TemperaturePreset("PLA", 210, 60),
        new TemperaturePreset("PETG", 240, 80),
        new TemperaturePreset("ABS", 250, 100),
        new TemperaturePreset("ASA", 260, 105),
        new TemperaturePreset("Cooldown", 0, 0)
    };

    [RelayCommand]
    private void SetNozzle()
    {
        printerControlService?.SetTemperature(TargetNozzleTemp, "extruder");
    }

    [RelayCommand]
    private void SetBed()
    {
        printerControlService?.SetTemperature(TargetBedTemp, "heater_bed");
    }

    [RelayCommand]
    private void ApplyPreset(TemperaturePreset preset)
    {
        TargetNozzleTemp = preset.Nozzle;
        TargetBedTemp = preset.Bed;
        
        SetNozzle();
        SetBed();
    }
}
public record TemperaturePreset(string Name, int Nozzle, int Bed);