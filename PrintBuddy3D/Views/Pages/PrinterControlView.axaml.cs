using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using WebViewControl;
using Microsoft.Extensions.DependencyInjection;
using PrintBuddy3D.ViewModels.Pages;

namespace PrintBuddy3D.Views.Pages;

public partial class PrinterControlView : UserControl
{
    private bool _isWebModeEnabled;
    public PrinterControlView()
    {
        InitializeComponent();
    }

    // This is a test, need to be edited to use the VM and get the URL from it.
    //TODO: Implement the VM to get the URL from it. Also add local klipper control panel instead of the webview.
    private void ReloadWebView()
    {
        // For some reason the WebView doesn't load the second time when navigating back to this page.
        // This is a workaround to force the WebView to load succesfully.
        // Also mby i will leave it like this, bcs this will be second option to connect to the printer.
        // TODO: Find a better solution. (probably won't find one)
        try
        {
            /*
            var parent = WebView.Parent as Panel;
            if (parent is null) return;

            parent.Children.Remove(WebView);

            WebView = new WebView();
            var printersListVm = App.Services.GetRequiredService<PrintersListViewModel>();
            if (printersListVm.CurrentContent is PrinterControlViewModel printerControlVm)
            {
                var printerAddress = printerControlVm.Printer.Address;
                Console.WriteLine(printerAddress);
                if (printerAddress != null) WebView.Url = new Uri(printerAddress);
            }
            parent.Children.Add(WebView);*/
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error while reloading WebView: " + ex.Message);
        }
    }
    
    private void SwitchModes(object? sender, RoutedEventArgs e)
    {
        _isWebModeEnabled = !_isWebModeEnabled;
        if (_isWebModeEnabled)
        {
            WebPanel.IsVisible = true;
            ReloadWebView();
            //KlipperControlPanel.IsVisible = false;
        }
        else
        {
            WebPanel.IsVisible = false;
            //KlipperControlPanel.IsVisible = true;
        }
    }
}