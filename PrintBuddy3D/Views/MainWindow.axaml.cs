using Avalonia.Controls;
using SukiUI.Controls;

namespace PrintBuddy3D.Views;

public partial class MainWindow : SukiWindow
{
    public bool ForceClose = false; // Probably should add some option to change this but idc for now ngl
    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (!ForceClose)
        {
            e.Cancel = true;
            Hide();
        }
        else
        {
            base.OnClosing(e);
        }
    }
}