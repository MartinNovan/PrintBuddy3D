using CommunityToolkit.Mvvm.ComponentModel;

namespace PrintBuddy3D.ViewModels.Pages;

public partial class PrinterControlViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _url = "https://www.klipper.local";
}