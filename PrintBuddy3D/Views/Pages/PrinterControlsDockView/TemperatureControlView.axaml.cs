using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Media;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Avalonia;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using SkiaSharp;
using SukiUI.Animations;

namespace PrintBuddy3D.Views.Pages.PrinterControlsDockView;

public partial class TemperatureControlView : UserControl
{
    public TemperatureControlView()
    {
        InitializeComponent();
    }
}