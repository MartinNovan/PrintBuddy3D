using System;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PrintBuddy3D.Models;

namespace PrintBuddy3D.ViewModels.Pages;

public partial class PrintMaterialsViewModel : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<Filament> _filaments = new();
    [ObservableProperty]
    private ObservableCollection<Resin> _resins = new();
    [ObservableProperty] 
    private ObservableCollection<String> _materialTypes = new()
    {
        "Filament",
        "Resin"
    };
    [ObservableProperty]
    private String? _selectedMaterialType = "Filament";

    [ObservableProperty] 
    private Filament _newFilament = new();
    [ObservableProperty] 
    private Resin _newResin = new();

    public bool IsFilamentSelected => SelectedMaterialType == "Filament";
    public bool IsResinSelected => SelectedMaterialType == "Resin";
    partial void OnSelectedMaterialTypeChanged(string? oldValue, string? newValue)
    {
        OnPropertyChanged(nameof(IsFilamentSelected));
        OnPropertyChanged(nameof(IsResinSelected));
    }

    [RelayCommand]
    private void AddMaterial()
    {
        if (SelectedMaterialType == "Filament")
        {
            Filaments.Add(new Filament
            {
                Manufacture = string.IsNullOrEmpty(NewFilament.Manufacture) ? "Unknown" : NewFilament.Manufacture,
                Name = string.IsNullOrEmpty(NewFilament.Name) ? "No Name" : NewFilament.Name,
                Color = string.IsNullOrEmpty(NewFilament.Color) ? "No Color" : NewFilament.Color,
                Weight = NewFilament.Weight > 0 ? NewFilament.Weight : 1200, // Default to 1200g if not set
                Price = NewFilament.Price > 0 ? NewFilament.Price : 0, // Default to 0 if not set
                SpoolWeight = NewFilament.SpoolWeight > 0 ? NewFilament.SpoolWeight : 200, // Default to 200g if not set
                Diameter = NewFilament.Diameter > 0 ? NewFilament.Diameter : 1.75, // Default to 1.75mm if not set
                Density = NewFilament.Density > 0 ? NewFilament.Density : 1.24 // Default to 1.24g/cm³ if not set
            });
        }
        else if (SelectedMaterialType == "Resin")
        {
            Resins.Add(new Resin
            {
                Manufacture = string.IsNullOrEmpty(NewResin.Manufacture) ? "Unknown" : NewResin.Manufacture,
                Name = string.IsNullOrEmpty(NewResin.Name) ? "No Name" : NewResin.Name,
                Color = string.IsNullOrEmpty(NewResin.Color) ? "No Color" : NewResin.Color,
                Weight = NewResin.Weight > 0 ? NewResin.Weight : 1000, // Default to 1000g if not set
                Price = NewResin.Price > 0 ? NewResin.Price : 0 // Default to 0.0 if not set
            });
        }
        // Clear the input fields after adding
        NewFilament = new Filament();
        NewResin = new Resin();
    }
    [RelayCommand]
    private void RemoveFilament(Filament filament)
        => Filaments.Remove(filament);

    [RelayCommand]
    private void RemoveResin(Resin resin)
        => Resins.Remove(resin);
}