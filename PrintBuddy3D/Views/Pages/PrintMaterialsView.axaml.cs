using Avalonia.Controls;
using Avalonia.Interactivity;
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
