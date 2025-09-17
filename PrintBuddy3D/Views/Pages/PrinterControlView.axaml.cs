using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.X11.Interop;
using PrintBuddy3D.Controls;
using PrintBuddy3D.ViewModels.Pages;

namespace PrintBuddy3D.Views.Pages;

public partial class PrinterControlView : UserControl
{
    public PrinterControlView()
    {
        InitializeComponent();
    }
}