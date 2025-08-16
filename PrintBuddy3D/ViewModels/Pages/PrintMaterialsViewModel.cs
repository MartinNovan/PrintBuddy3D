using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks; // added
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PrintBuddy3D.Models;
using PrintBuddy3D.Services;

namespace PrintBuddy3D.ViewModels.Pages;

public partial class PrintMaterialsViewModel : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<FilamentModel> _filaments = new();
    [ObservableProperty]
    private ObservableCollection<ResinModel> _resins = new();
    [ObservableProperty] 
    private ObservableCollection<String> _materialTypes = new()
    {
        "Filament",
        "Resin"
    };
    [ObservableProperty]
    private String? _selectedMaterialType = "Filament";

    [ObservableProperty] 
    private FilamentModel _newFilamentModel = new();
    [ObservableProperty] 
    private ResinModel _newResinModel = new();

    public bool IsFilamentSelected => SelectedMaterialType == "Filament";
    public bool IsResinSelected => SelectedMaterialType == "Resin";
    
    private readonly IPrintMaterialService _printMaterialService;
    
    partial void OnSelectedMaterialTypeChanged(string? oldValue, string? newValue)
    {
        OnPropertyChanged(nameof(IsFilamentSelected));
        OnPropertyChanged(nameof(IsResinSelected));
    }

    public PrintMaterialsViewModel(IPrintMaterialService printMaterialService)
    {
        _printMaterialService = printMaterialService;
        LoadData();
    }
    
    private async void LoadData()
    {
        try
        {
            Filaments = await _printMaterialService.GetFilamentsAsync();
            Resins = await _printMaterialService.GetResinsAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine("There was error while loading filaments:" + ex);
        }
    }
    
    [RelayCommand]
    private async Task AddMaterial()
    {
        if (SelectedMaterialType == "FilamentModel")
        {
            var fil = new FilamentModel
            {
                Manufacture = string.IsNullOrEmpty(NewFilamentModel.Manufacture) ? "Unknown" : NewFilamentModel.Manufacture,
                Name = string.IsNullOrEmpty(NewFilamentModel.Name) ? "No Name" : NewFilamentModel.Name,
                Color = string.IsNullOrEmpty(NewFilamentModel.Color) ? "No Color" : NewFilamentModel.Color,
                Weight = NewFilamentModel.Weight > 0 ? NewFilamentModel.Weight : 1200, // Default to 1200g if not set
                Price = NewFilamentModel.Price > 0 ? NewFilamentModel.Price : 0, // Default to 0 if not set
                SpoolWeight = NewFilamentModel.SpoolWeight > 0 ? NewFilamentModel.SpoolWeight : 200, // Default to 200g if not set
                Diameter = NewFilamentModel.Diameter > 0 ? NewFilamentModel.Diameter : 1.75, // Default to 1.75mm if not set
                Density = NewFilamentModel.Density > 0 ? NewFilamentModel.Density : 1.24 // Default to 1.24g/cm³ if not set
            };
            await _printMaterialService.UpsertFilamentAsync(fil);
            fil.DbHash = fil.Hash;
            Filaments.Add(fil);
        }
        else if (SelectedMaterialType == "Resin")
        {
            var res = new ResinModel
            {
                Manufacture = string.IsNullOrEmpty(NewResinModel.Manufacture) ? "Unknown" : NewResinModel.Manufacture,
                Name = string.IsNullOrEmpty(NewResinModel.Name) ? "No Name" : NewResinModel.Name,
                Color = string.IsNullOrEmpty(NewResinModel.Color) ? "No Color" : NewResinModel.Color,
                Weight = NewResinModel.Weight > 0 ? NewResinModel.Weight : 1000, // Default to 1000g if not set
                Price = NewResinModel.Price > 0 ? NewResinModel.Price : 0 // Default to 0.0 if not set
            };
            await _printMaterialService.UpsertResinAsync(res);
            res.DbHash = res.Hash;
            Resins.Add(res);
        }
        // Clear the input fields after adding
        NewFilamentModel = new FilamentModel();
        NewResinModel = new ResinModel();
    }

    [RelayCommand]
    private async Task RemoveFilament(FilamentModel filamentModel)
    {
        await _printMaterialService.RemoveFilamentAsync(filamentModel);
        Filaments.Remove(filamentModel);
    }

    [RelayCommand]
    private async Task RemoveResin(ResinModel resinModel)
    {
        await _printMaterialService.RemoveResinAsync(resinModel);
        Resins.Remove(resinModel);
    }
}