using System;
using Avalonia.Controls;
using Avalonia.Input;
using Microsoft.Extensions.DependencyInjection;
using PrintBuddy3D.Models;
using PrintBuddy3D.ViewModels.Pages;

namespace PrintBuddy3D.Views.Pages;

public partial class PrintersListView : UserControl
{
    public PrintersListView()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<PrintersListViewModel>();
    }
}