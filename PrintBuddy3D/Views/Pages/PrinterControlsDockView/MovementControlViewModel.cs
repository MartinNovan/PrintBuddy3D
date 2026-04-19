using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dock.Model.Mvvm.Controls;
using PrintBuddy3D.Services;

namespace PrintBuddy3D.Views.Pages.PrinterControlsDockView;

public partial class MovementControlViewModel(IPrinterControlService contextPrinterControlService) : Document
{
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
        var axis = axisAndDir.Substring(0, 1);
        var dir = axisAndDir.Substring(1, 1);
        
        var distance = StepSize;
        if (dir == "-") distance *= -1;

        var speed = axis == "Z" ? ZFeedRate : FeedRate;

        contextPrinterControlService.Move(axis, distance, speed);
    }

    [RelayCommand]
    private void Home(string axis)
    {
        contextPrinterControlService.Home(axis);
    }
    
    [RelayCommand]
    private void DisableMotors()
    {
        contextPrinterControlService.DisableMotors();
    }
    
    [RelayCommand]
    private void SetStep(string value)
    {
        if (double.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var result))
        {
            IsCustomStepEnable = false;
            StepSize = result;
        }
        else
        {
            IsCustomStepEnable = true;
        }
    }

    public void Dispose()
    {
    }
}