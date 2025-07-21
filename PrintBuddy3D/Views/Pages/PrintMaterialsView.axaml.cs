using System;
using System.Collections.ObjectModel;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using PrintBuddy3D.Models;
using PrintBuddy3D.ViewModels.Pages;

namespace PrintBuddy3D.Views.Pages;

public partial class PrintMaterialsView : UserControl
{
    public PrintMaterialsView()
    {
        InitializeComponent();
        DataContext = new PrintMaterialsViewModel();
    }
}
