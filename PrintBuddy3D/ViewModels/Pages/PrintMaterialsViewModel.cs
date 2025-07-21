using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PrintBuddy3D.Models;

namespace PrintBuddy3D.ViewModels.Pages;

public partial class PrintMaterialsViewModel : ViewModelBase
{
    [ObservableProperty]
    private MaterialType _selectedMaterialType = MaterialType.Filament;
    [ObservableProperty]
    private ObservableCollection<MaterialType> _materialTypes = new()
    {
        MaterialType.Filament,
        MaterialType.Resin,
        MaterialType.Powder,
        MaterialType.Pellets
    };

    [ObservableProperty]
    private PrintMaterial _editableMaterial = new();

    [RelayCommand]
    private void AddMaterial()
    {
        switch (SelectedMaterialType)
        {
            case MaterialType.Filament: {
                Filaments.Add(EditableMaterial.PrintMaterialFilament(
                    EditableMaterial.Manufacture,
                    EditableMaterial.Name,
                    EditableMaterial.Color,
                    EditableMaterial.MaterialHousingWeight,
                    EditableMaterial.Diameter,
                    EditableMaterial.Density,
                    EditableMaterial.Weight,
                    EditableMaterial.Price));
                break;
            }

            case MaterialType.Resin:
            {
                Resins.Add(EditableMaterial.PrintMaterialResin(
                    EditableMaterial.Manufacture,
                    EditableMaterial.Name,
                    EditableMaterial.Color,
                    EditableMaterial.MaterialHousingWeight,
                    EditableMaterial.Weight,
                    EditableMaterial.Price));
                break;
            }

            case MaterialType.Powder:
            {
                Powders.Add(EditableMaterial.PrintMaterialPowder(
                    EditableMaterial.Manufacture,
                    EditableMaterial.Name,
                    EditableMaterial.Color,
                    EditableMaterial.MaterialHousingWeight,
                    EditableMaterial.Weight,
                    EditableMaterial.Price));
                break;
            }

            case MaterialType.Pellets:
            {
                Pellets.Add(EditableMaterial.PrintMaterialPellets(
                    EditableMaterial.Manufacture,
                    EditableMaterial.Name,
                    EditableMaterial.Color,
                    EditableMaterial.Weight,
                    EditableMaterial.Price));
                break;
            }
        }
        EditableMaterial = new PrintMaterial { MaterialType = SelectedMaterialType };
    }
    
    
    public ObservableCollection<PrintMaterial> Filaments { get; } = new()
    {
        new PrintMaterial().PrintMaterialFilament("Prusa", "Prusament PLA Galaxy Black", "Black", 300, 1.75, 1.24, 1000, 20),
        new PrintMaterial().PrintMaterialFilament("Prusa", "Prusament PLA Galaxy Black", "Black", 300, 1.75, 1.24, 1000, 20),
        new PrintMaterial().PrintMaterialFilament("Prusa", "Prusament PLA Galaxy Black", "Black", 300, 1.75, 1.24, 1000, 20),
        new PrintMaterial().PrintMaterialFilament("Prusa", "Prusament PLA Galaxy Black", "Black", 300, 1.75, 1.24, 1000, 20),
        new PrintMaterial().PrintMaterialFilament("Prusa", "Prusament PLA Galaxy Black", "Black", 300, 1.75, 1.24, 1000, 20),
        new PrintMaterial().PrintMaterialFilament("Prusa", "Prusament PLA Galaxy Black", "Black", 300, 1.75, 1.24, 1000, 20),
        new PrintMaterial().PrintMaterialFilament("Prusa", "Prusament PLA Galaxy Black", "Black", 300, 1.75, 1.24, 1000, 20),
    };
    public ObservableCollection<PrintMaterial> Resins { get; } = new()
    {
        new PrintMaterial().PrintMaterialResin("Prusa", "Prusament Resin Grey", "Grey", 100, 500, 20),
        new PrintMaterial().PrintMaterialResin("Prusa", "Prusament Resin Grey", "Grey", 100, 500, 20),
        new PrintMaterial().PrintMaterialResin("Prusa", "Prusament Resin Grey", "Grey", 100, 500, 20),
    };
    public ObservableCollection<PrintMaterial> Powders { get; } = new()
    {
        new PrintMaterial().PrintMaterialPowder("Prusa", "Prusament Powder Grey", "Grey", 500, 500, 20),
        new PrintMaterial().PrintMaterialPowder("Prusa", "Prusament Powder Grey", "Grey", 500, 500, 20),
        new PrintMaterial().PrintMaterialPowder("Prusa", "Prusament Powder Grey", "Grey", 500, 500, 20),
    };
    public ObservableCollection<PrintMaterial> Pellets { get; } = new()
    {
        new PrintMaterial().PrintMaterialPellets("Prusa", "Prusament PLA Galaxy Black", "Black", 1000, 1000),
        new PrintMaterial().PrintMaterialPellets("Prusa", "Prusament PLA Galaxy Black", "Black", 1000, 1000),
        new PrintMaterial().PrintMaterialPellets("Prusa", "Prusament PLA Galaxy Black", "Black", 1000, 1000),
    };
}