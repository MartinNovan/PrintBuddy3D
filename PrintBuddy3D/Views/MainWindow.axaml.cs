using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using PrintBuddy3D.ViewModels;
using SukiUI.Controls;

namespace PrintBuddy3D;

public partial class MainWindow : SukiWindow
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }

    private void SettingButtonClicked(object? sender, RoutedEventArgs e)
    {
        SideMenu.SelectedItem = SideMenu.Items.OfType<SukiSideMenuItem>().FirstOrDefault(i => i.Header?.ToString() == "Settings");
    }
}