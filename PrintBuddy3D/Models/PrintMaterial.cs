using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PrintBuddy3D.Models;
public class PrintMaterial : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public Guid Id { get; init; }
    
    private string _manufacture = string.Empty;
    public string Manufacture
    {
        get => _manufacture;
        set
        {
            if (_manufacture != value)
            {
                _manufacture = value;
                OnPropertyChanged();
            }
        }
    }

    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set
        {
            if (_name != value)
            {
                _name = value;
                OnPropertyChanged();
            }
        }
    }

    private MaterialType _materialType;
    public MaterialType MaterialType
    {
        get => _materialType;
        set
        {
            if (_materialType != value)
            {
                _materialType = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Length));
            }
        }
    }

    private string? _color;
    public string? Color
    {
        get => _color;
        set
        {
            if (_color != value)
            {
                _color = value;
                OnPropertyChanged();
            }
        }
    }

    private double? _diameter;
    public double? Diameter
    {
        get => _diameter;
        set
        {
            if (_diameter != value)
            {
                _diameter = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Length));
            }
        }
    }

    private double? _density;
    public double? Density
    {
        get => _density;
        set
        {
            if (_density != value)
            {
                _density = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Length));
            }
        }
    }

    private int _materialHousingWeight;
    public int MaterialHousingWeight
    {
        get => _materialHousingWeight;
        set
        {
            if (_materialHousingWeight != value)
            {
                _materialHousingWeight = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(RemainingWeight));
                OnPropertyChanged(nameof(Length));
            }
        }
    }

    private int? _weight;
    public int? Weight
    {
        get => _weight;
        set
        {
            if (_weight != value)
            {
                _weight = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(RemainingWeight));
                OnPropertyChanged(nameof(Length));
            }
        }
    }

    public int? RemainingWeight => Weight.HasValue ? Weight.Value - MaterialHousingWeight : 0;

    public double? Length => MaterialType == MaterialType.Filament && Diameter.HasValue && Density.HasValue && Weight.HasValue
        ? (int?)((Weight.Value - MaterialHousingWeight) / (Math.PI * Math.Pow(Diameter.Value / 2, 2) * Density.Value))
        : null;

    private int? _price;
    public int? Price
    {
        get => _price;
        set
        {
            if (_price != value)
            {
                _price = value;
                OnPropertyChanged();
            }
        }
    }

    public PrintMaterial PrintMaterialFilament(string manufacture, string name, string? color = null, int? spoolWeight = null, double? diameter = null, double? density = null, int? weight = null, int? price = null)
    {
        return new PrintMaterial
        {
            Id = Guid.NewGuid(),
            Manufacture = manufacture,
            Name = name,
            MaterialType = MaterialType.Filament,
            Color = color,
            MaterialHousingWeight = spoolWeight ?? 0,
            Diameter = diameter,
            Density = density,
            Weight = weight,
            Price = price
        };
    }
    public PrintMaterial PrintMaterialResin(string manufacture, string name, string? color = null, int? cartridgeWeight = null, int? weight = null, int? price = null)
    {
        return new PrintMaterial
        {
            Id = Guid.NewGuid(),
            Manufacture = manufacture,
            Name = name,
            MaterialType = MaterialType.Resin,
            Color = color,
            MaterialHousingWeight = cartridgeWeight ?? 0,
            Weight = weight,
            Price = price
        };
    }
    public PrintMaterial PrintMaterialPowder(string manufacture, string name, string? color = null, int? cartridgeWeight = null, int? weight = null, int? price = null)
    {
        return new PrintMaterial
        {
            Id = Guid.NewGuid(),
            Manufacture = manufacture,
            Name = name,
            MaterialType = MaterialType.Powder,
            Color = color,
            MaterialHousingWeight = cartridgeWeight ?? 0,
            Weight = weight,
            Price = price
        };
    }
    public PrintMaterial PrintMaterialPellets(string manufacture, string name, string? color = null, int? weight = null, int? price = null)
    {
        return new PrintMaterial
        {
            Id = Guid.NewGuid(),
            Manufacture = manufacture,
            Name = name,
            MaterialType = MaterialType.Pellets,
            Color = color,
            Weight = weight,
            Price = price
        };
    }
}

public enum MaterialType
{
    Filament = 0,
    Resin = 1,
    Powder = 2,
    Pellets = 3
}