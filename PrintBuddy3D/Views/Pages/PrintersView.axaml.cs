using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using PrintBuddy3D.ViewModels.Pages;

namespace PrintBuddy3D.Views.Pages;

public partial class PrintersView : UserControl
{
    public PrintersView()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<PrintersViewModel>();
    }
}