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

    private void RemoveFilamentOnClick(object? sender, RoutedEventArgs e)
    {
        if( DataContext is not PrintMaterialsViewModel vm) return;
        if (sender is not Button button) return;
        if (button.DataContext is not Filament filament) return;
        vm.Filaments.Remove(filament);
    }
    private void RemoveResinOnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not PrintMaterialsViewModel vm) return;
        if (sender is not Button button) return;
        if (button.DataContext is not Resin resin) return;
        vm.Resins.Remove(resin);
    }
}
