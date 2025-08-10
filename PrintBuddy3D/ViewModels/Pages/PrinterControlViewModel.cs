using CommunityToolkit.Mvvm.ComponentModel;
using PrintBuddy3D.Models;

namespace PrintBuddy3D.ViewModels.Pages;

public partial class PrinterControlViewModel : ViewModelBase
{
    [ObservableProperty]
    private PrinterModel _printer;
}