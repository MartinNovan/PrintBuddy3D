using System;
using System.Diagnostics;
using System.Linq;
using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PrintBuddy3D.Services;
using PrintBuddy3D.ViewModels.Pages;
using PrintBuddy3D.Views;
using SukiUI;
using SukiUI.Dialogs;
using SukiUI.Enums;
using SukiUI.Models;
using SukiUI.Toasts;

namespace PrintBuddy3D.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    public IAvaloniaReadOnlyList<PageBase> Pages { get; }
    
    [ObservableProperty] private PageBase _activePage;
    [ObservableProperty] private bool _isAlwaysOnTop;
    [ObservableProperty] private SukiBackgroundStyle _backgroundStyle;
    [ObservableProperty] private SukiTheme _themes;
    [ObservableProperty] private IAvaloniaReadOnlyList<SukiBackgroundStyle> _backgroundStyles;
    
    public ISukiDialogManager DialogManager { get; }
    public ISukiToastManager ToastManager { get; }
    private readonly IAppDataService _appDataService;
    private AboutWindow? _aboutWindow;

    public MainWindowViewModel(HomeViewModel homeViewModel, PrintersListViewModel printersListViewModel, FilamentsViewModel filamentsViewModel,
        GuidesViewModel guidesViewModel, IAppDataService appDataService, ISukiDialogManager dialogManager, ISukiToastManager toastManager)
    {
        Pages = new AvaloniaList<PageBase>(
            new PageBase[] { homeViewModel, printersListViewModel, filamentsViewModel, guidesViewModel }
                .OrderBy(p => p.Index)
        );
        _activePage = Pages[0];
        ToastManager = toastManager;
        DialogManager = dialogManager;
        _appDataService = appDataService;
        _themes = SukiTheme.GetInstance();
        _backgroundStyles = new AvaloniaList<SukiBackgroundStyle>(Enum.GetValues<SukiBackgroundStyle>());
        BackgroundStyle = _appDataService.LoadBackground("Background");
        Themes.ChangeColorTheme(_appDataService.LoadTheme("Theme"));
    }
    partial void OnBackgroundStyleChanged(SukiBackgroundStyle value)
    {
        _appDataService.SaveConfigValue("Background", value.ToString());
    }

    public SukiColorTheme CurrentTheme
    {
        get => Themes.ActiveColorTheme;
        set
        {
            // Pokud se vybraná hodnota liší od aktuální, změníme téma a uložíme
            if (value != null && value != Themes.ActiveColorTheme)
            {
                Themes.ChangeColorTheme(value);
                _appDataService.SaveConfigValue("Theme", value.DisplayName);
                OnPropertyChanged(); // Oznámíme UI, že se změna provedla
            }
        }
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
        if (_aboutWindow != null)
        {
            _aboutWindow.Activate();
            return;
        }
        _aboutWindow = new AboutWindow();
        _aboutWindow.Closed += (_, _) => _aboutWindow = null;
        _aboutWindow.Show();
    }
}