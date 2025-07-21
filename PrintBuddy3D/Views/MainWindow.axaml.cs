using Avalonia.Controls;
using PrintBuddy3D.ViewModels;
using PrintBuddy3D.ViewModels.Pages;
using SukiUI.Controls;
using SukiUI.Enums;
using SukiUI.Models;

namespace PrintBuddy3D.Views;

public partial class MainWindow : SukiWindow
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }
    
    private void SelectedTheme(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm) return;
        if (e.Source is not ComboBox comboBox) return;
        if (comboBox.SelectedItem is not SukiColorTheme selectedTheme) return;
        vm.ChangeTheme(selectedTheme);
    }
    
    private void SelectedBackgroundStyle(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm) return;
        if (e.Source is not ComboBox comboBox) return;
        if (comboBox.SelectedItem is not SukiBackgroundStyle selectedStyle) return;
        vm.ChangeBackgroundStyle(selectedStyle);
    }
}