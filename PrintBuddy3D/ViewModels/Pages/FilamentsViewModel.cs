using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks; // added
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using PrintBuddy3D.Models;
using PrintBuddy3D.Services;

namespace PrintBuddy3D.ViewModels.Pages;

public partial class FilamentsViewModel : PageBase
{
    [ObservableProperty]
    private ObservableCollection<FilamentModel> _filaments = new();
    [ObservableProperty] 
    private FilamentModel _newFilamentModel = new();
    
    private readonly IPrintMaterialService _printMaterialService;
    public FilamentsViewModel(IPrintMaterialService printMaterialService) : base("Filaments", MaterialIconKind.FreehandLine, 2)
    {
        _printMaterialService = printMaterialService;
        LoadFilaments();
    }
    
    private async void LoadFilaments()
    {
        try
        {
            Filaments = await _printMaterialService.GetFilamentsAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine("There was error while loading filaments:" + ex);
        }
    }
    
    [RelayCommand]
    private async Task AddMaterial()
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
        fil.PropertyChanged += async (_, _) =>
        {
            if (fil.Hash != fil.DbHash)
            {
                await _printMaterialService.UpsertFilamentAsync(fil);
                fil.DbHash = fil.Hash;
            }
        };
        Filaments.Add(fil);
        // Clear the input fields after adding
        NewFilamentModel = new FilamentModel();
    }

    [RelayCommand]
    private async Task RemoveFilament(FilamentModel filamentModel)
    {
        await _printMaterialService.RemoveFilamentAsync(filamentModel);
        Filaments.Remove(filamentModel);
    }
}