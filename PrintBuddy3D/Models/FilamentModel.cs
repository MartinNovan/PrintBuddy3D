using System;
using PrintBuddy3D.Services;

namespace PrintBuddy3D.Models;

public class FilamentModel : ModelBase
{
    public Guid Id { get; init; } = Guid.NewGuid(); // Unique identifier for each print material
    public int DbHash { get; set; } // Hash for database tracking, used to detect changes
    private string? _manufacture;
    public string? Manufacture
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

    private string? _name;
    public string? Name
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

    private int _weight;
    public int Weight
    {
        get => _weight;
        set
        {
            if (_weight != value && value >= 0) // Ensure weight is non-negative
            {
                _weight = value;
                OnPropertyChanged();
            }
        }
    }

    private double _price;
    public double Price
    {
        get => _price;
        set
        {
            if (Math.Abs(_price - value) > 0.0001 && value >= 0) // Ensure price is non-negative
            {
                _price = value;
                OnPropertyChanged();
            }
        }
    }
    public int Hash
    {
        get
        {
            var hash = new HashCode();
            hash.Add(Id);
            hash.Add(Weight);
            hash.Add(Price);
            hash.Add(Manufacture);
            hash.Add(Name);
            hash.Add(Color);
            hash.Add(Diameter);
            hash.Add(Density);
            hash.Add(SpoolWeight);
            return hash.ToHashCode();
        }
    }

    private double _diameter;
    public double Diameter
    {
        get => _diameter;
        set
        {
            if (value <= 0) throw new ArgumentOutOfRangeException(nameof(Diameter), "Diameter must be greater than 0.");
            if (Math.Abs(_diameter - value) > 0.0001)
            {
                _diameter = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Length));
            }
        }
    }

    private double _density;
    public double Density
    {
        get => _density;
        set
        {
            if (value <= 0) throw new ArgumentOutOfRangeException(nameof(Density), "Density must be greater than 0.");
            if (Math.Abs(_density - value) > 0.0001)
            {
                _density = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Length));
            }
        }
    }

    private int _spoolWeight;
    public int SpoolWeight
    {
        get => _spoolWeight;
        set
        {
            if (_spoolWeight != value && value >= 0) // Ensure spool weight is non-negative
            {
                _spoolWeight = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(RemainingWeight));
                OnPropertyChanged(nameof(Length));
            }
        }
    }

    public int? RemainingWeight => Weight - SpoolWeight;

    public double? Length =>
        Diameter > 0 && Density > 0
            ? Math.Round((Weight - SpoolWeight) / (Math.PI * Math.Pow(Diameter / 2, 2) * Density), 2)
            : null;
}