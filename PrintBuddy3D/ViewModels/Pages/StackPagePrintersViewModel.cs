using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using PrintBuddy3D.Views.Pages;

namespace PrintBuddy3D.ViewModels.Pages;

public partial class StackPagePrintersViewModel
{
    public PrintersListView printeListView { get; set; } = new();
}