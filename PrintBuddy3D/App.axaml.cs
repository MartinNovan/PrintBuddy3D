using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using PrintBuddy3D.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using PrintBuddy3D.Services;
using AvaloniaWebView;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using PrintBuddy3D.Common;
using PrintBuddy3D.Views.Pages.PrinterControlsDockView;
using PrintBuddy3D.Views.Pages;
using PrintBuddy3D.Views;
using PrintBuddy3D.ViewModels.Pages;

namespace PrintBuddy3D;

public class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;
    public static WindowViews WindowViews { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var (services, windowViews) = ConfigureServices();
        Services = services;
        WindowViews = windowViews;
        AvaloniaWebViewBuilder.Initialize(default);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = new MainWindow
            {
                DataContext = Services.GetRequiredService<MainWindowViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static (IServiceProvider, WindowViews) ConfigureServices()
    {
        var services = new ServiceCollection();
        var windowViews = new WindowViews();

        // Services
        services.AddSingleton<IAppDataService, AppDataService>();
        services.AddSingleton<IPrintMaterialService, PrintMaterialService>();
        services.AddSingleton<IPrintersService, PrintersService>();
        services.AddSingleton<IPrinterMonitoringService, PrinterMonitoringService>();
        services.AddSingleton<INotificationService, NotificationService>();
        services.AddSingleton<ISukiDialogManager, SukiDialogManager>();
        services.AddSingleton<ISukiToastManager, SukiToastManager>();

        // ViewModels and Views
        windowViews.AddView<MainWindow, MainWindowViewModel>(services);
        windowViews.AddView<PrintersListView, PrintersListViewModel>(services);
        windowViews.AddView<FilamentsView, FilamentsViewModel>(services);
        windowViews.AddView<HomeView, HomeViewModel>(services);
        windowViews.AddView<GuidesView, GuidesViewModel>(services);
        windowViews.AddView<PrinterControlView, PrinterControlViewModel>(services);
        windowViews.AddView<PrinterEditorView, PrinterEditorViewModel>(services);
        
        // Dock control ViewModels and Views
        windowViews.AddView<MainControlView, MainControlViewModel>(services);
        windowViews.AddView<MovementControlView, MovementControlViewModel>(services);
        windowViews.AddView<TemperatureControlView, TemperatureControlViewModel>(services);
        windowViews.AddView<PrinterConsoleControlView, PrinterConsoleControlViewModel>(services);

        return (services.BuildServiceProvider(), windowViews);
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}