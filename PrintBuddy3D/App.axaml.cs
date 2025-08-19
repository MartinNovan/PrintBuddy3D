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

namespace PrintBuddy3D;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = default!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        Services = ConfigureServices();
        AvaloniaWebViewBuilder.Initialize(default);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = new Views.MainWindow
            {
                DataContext = Services.GetRequiredService<MainWindowViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Services
        services.AddSingleton<IAppDataService, AppDataService>();
        services.AddSingleton<IPrintMaterialService, PrintMaterialService>();
        services.AddSingleton<IPrintersService, PrintersService>();
        services.AddSingleton<INotificationService, NotificationService>();
        services.AddSingleton<ISukiDialogManager, SukiDialogManager>();
        services.AddSingleton<ISukiToastManager, SukiToastManager>();

        // ViewModels
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<ViewModels.Pages.PrintersListViewModel>();
        services.AddSingleton<ViewModels.Pages.PrintMaterialsViewModel>();
        services.AddSingleton<ViewModels.Pages.HomeViewModel>();
        services.AddSingleton<ViewModels.Pages.GuidesViewModel>();

        return services.BuildServiceProvider();
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