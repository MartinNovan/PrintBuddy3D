using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dock.Model.Mvvm.Controls;
using PrintBuddy3D.Services;

namespace PrintBuddy3D.Views.Pages.PrinterControlsDockView;

public partial class MovementControlViewModel(IPrinterControlService contextPrinterControlService) : Tool
{
    private readonly KlipperPrinterControlService? _klipperService = contextPrinterControlService as KlipperPrinterControlService;
    
    [ObservableProperty] private double _stepSize = 10;
    [ObservableProperty] private bool _isCustomStepEnable;
    //TODO Take the values under here from printer online (so i need firstly to get them from printer), for now this is only test value, also when value changes send this to the printer
    [ObservableProperty] private int _feedRate = 6000;
    [ObservableProperty] private int _zFeedRate = 1500;
    [ObservableProperty] private int _speedFactor = 100;
    [ObservableProperty] private int _velocity = 400;
    [ObservableProperty] private int _acceleration = 4000;
    [ObservableProperty] private int _squareCornerVelocity = 5;
    [ObservableProperty] private int _minCruiseRatio = 50;
    

    [RelayCommand]
    private void Move(string axisAndDir)
    {
        if (_klipperService == null) return;

        string axis = axisAndDir.Substring(0, 1);
        string dir = axisAndDir.Substring(1, 1);
        
        double distance = StepSize;
        if (dir == "-") distance *= -1;

        int speed = axis == "Z" ? ZFeedRate : FeedRate;

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
            IsCustomStepEnable = false;
            StepSize = result;
        }
        else
        {
            IsCustomStepEnable = true;
        }
    }
}