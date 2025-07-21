using System;
using System.Diagnostics;
using System.Dynamic;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PrintBuddy3D.Services;
using PrintBuddy3D.ViewModels.Pages;
using SukiUI;
using SukiUI.Controls;
using SukiUI.Enums;
using SukiUI.Models;

namespace PrintBuddy3D.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] private ViewModelBase _currentPage;
    [ObservableProperty] private bool _isAlwaysOnTop;
    [ObservableProperty] private SukiBackgroundStyle _backgroundStyle;
    [ObservableProperty] private SukiTheme _themes;
    [ObservableProperty] private IAvaloniaReadOnlyList<SukiBackgroundStyle> _backgroundStyles;

    public MainWindowViewModel()
    {
        CurrentPage = new HomeViewModel();
        _themes = SukiTheme.GetInstance();
        _backgroundStyles = new AvaloniaList<SukiBackgroundStyle>(Enum.GetValues<SukiBackgroundStyle>());
        BackgroundStyle = AppDataService.Instance.LoadBackground("Background");
        Themes.ChangeColorTheme(AppDataService.Instance.LoadTheme("Theme"));
    }

    [RelayCommand]
    private void OpenWebsite()
    {
        var url = "https://github.com/MartinNovan/PrintBuddy3D";
        try
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            };
            process.Start();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Chyba při otevírání URL: {ex.Message}");
        }
    }
    
    [RelayCommand]
    private void AboutWindowShow()
    {
        var aboutWindow = new Views.AboutWindow();
        aboutWindow.Show();
    }

    [RelayCommand]
    public void ChangeTheme(SukiColorTheme theme)
    {
        if (theme.DisplayName == Themes.ActiveColorTheme?.DisplayName) return;
        Themes.ChangeColorTheme(theme);
        AppDataService.Instance.SaveConfigValue("Theme", theme.DisplayName);
    }
    
    [RelayCommand]
    public void ChangeBackgroundStyle(SukiBackgroundStyle style)
    {
        AppDataService.Instance.SaveConfigValue("Background", style.ToString());
        BackgroundStyle = style;
    }
}