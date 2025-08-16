using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;

namespace PrintBuddy3D.Views.Pages;

public partial class StackPagePrintersView : UserControl
{
    public StackPagePrintersView()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<ViewModels.Pages.StackPagePrintersViewModel>();
    }
}