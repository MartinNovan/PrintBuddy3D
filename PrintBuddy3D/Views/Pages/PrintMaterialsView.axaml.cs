using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using PrintBuddy3D.ViewModels.Pages;

namespace PrintBuddy3D.Views.Pages;

public partial class PrintMaterialsView : UserControl
{
    public PrintMaterialsView()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<PrintMaterialsViewModel>();
    }
}
