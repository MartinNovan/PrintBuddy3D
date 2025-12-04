using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dock.Model.Mvvm.Controls;
using PrintBuddy3D.Services;

namespace PrintBuddy3D.Views.Pages.PrinterControlsDockView;

public partial class MovementControlViewModel(IPrinterControlService contextPrinterControlService) : Tool
{
    private readonly KlipperPrinterControlService? _klipperService = contextPrinterControlService as KlipperPrinterControlService;
    
    [ObservableProperty] private double _stepSize = 10;
    //TODO make feedrate editable    
    [ObservableProperty] private int _feedRate = 3000;

    [RelayCommand]
    private void Move(string axisAndDir)
    {
        if (_klipperService == null) return;

        string axis = axisAndDir.Substring(0, 1);
        string dir = axisAndDir.Substring(1, 1);
        
        double distance = StepSize;
        if (dir == "-") distance *= -1;

        int speed = axis == "Z" ? 600 : FeedRate;

        _klipperService.Move(axis, distance, speed);
    }

    [RelayCommand]
    private void Home(string axis)
    {
        _klipperService?.Home(axis);
    }
    
    [RelayCommand]
    private void DisableMotors()
    {
        _klipperService?.DisableMotors();
    }
    
    [RelayCommand]
    private void SetStep(string value)
    {
        if (double.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double result))
        {
            StepSize = result;
        }
    }
}