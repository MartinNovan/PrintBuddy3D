using System;
using Avalonia.Controls;
using PrintBuddy3D.ViewModels;
using SukiUI.Controls;

namespace PrintBuddy3D.Views;

public partial class SshWindow : SukiWindow
{
    public SshWindow()
    {
        InitializeComponent();
    }
    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (DataContext is SshWindowViewModel vm)
        {
            vm.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(vm.Output))
                    this.FindControl<ScrollViewer>("OutputScroll")?.ScrollToEnd();
            };
        }
    }
    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        (DataContext as SshWindowViewModel)?.Dispose();
    }
}