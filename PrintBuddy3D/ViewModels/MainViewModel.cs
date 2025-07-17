using CommunityToolkit.Mvvm.ComponentModel;

namespace PrintBuddy3D.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty] private string _greeting = "Welcome to Avalonia!";
}