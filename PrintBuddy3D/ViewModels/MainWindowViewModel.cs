using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PrintBuddy3D.ViewModels.Pages;

namespace PrintBuddy3D.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] private ViewModelBase _currentPage;

    public MainWindowViewModel() : base()
    {
        CurrentPage = new HomeViewModel();
    }

    [RelayCommand]
    private void NavigateToHome() => CurrentPage = new HomeViewModel();

    [RelayCommand]
    private void NavigateToSettings() => CurrentPage = new SettingsViewModel();
}