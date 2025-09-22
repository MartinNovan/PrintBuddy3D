using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Dock.Model.Mvvm.Controls;

namespace PrintBuddy3D.Views.Pages.PrinterControlsDockView;

public partial class PrinterConsoleControlView : UserControl
{
    public PrinterConsoleControlView()
    {
        InitializeComponent();
    }
    private void OnTextBoxKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && DataContext is PrinterConsoleControlViewModel vm)
        {
            if (vm.SendCommandToPrinterCommand.CanExecute(null))
                vm.SendCommandToPrinterCommand.Execute(null);
        }
    }
}