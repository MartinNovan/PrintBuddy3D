using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace PrintBuddy3D.Models;

public class PrinterModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    public Guid Id { get; init; } = Guid.NewGuid(); // Unique identifier for each printer
    
    public int DbHash { get; set; } // Hash for database tracking, used to detect changes easily

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

    private string? _firmware;
    public string? Firmware
    {
        get => _firmware;
        set
        {
            if (_firmware != value)
            {
                _firmware = value;
                OnPropertyChanged();
            }
        }
    }

    private string? _connectionType;
    public string? ConnectionType
    {
        get => _connectionType;
        set
        {
            if (_connectionType != value)
            {
                _connectionType = value;
                OnPropertyChanged();
            }
        }
    }
    
    private string? _address;
    public string? Address
    {
        get => _address;
        set
        {
            if (_address != value)
            {
                _address = value;
                OnPropertyChanged();
            }
        }
    }
    
    private string? _serialPort;
    public string? SerialPort
    {
        get => _serialPort;
        set
        {
            if (_serialPort != value)
            {
                _serialPort = value;
                OnPropertyChanged();
            }
        }
    }
    
    private int _baudRate;
    public int BaudRate
    {
        get => _baudRate;
        set
        {
            if (_baudRate != value)
            {
                _baudRate = value;
                OnPropertyChanged();
            }
        }
    }
    
    private string? _serialNumber;
    public string? SerialNumber
    {
        get => _serialNumber;
        set
        {
            if (_serialNumber != value)
            {
                _serialNumber = value;
                OnPropertyChanged();
            }
        }
    }
    
    private string? _imagePath;
    public string? ImagePath
    {
        get
        {
            if (_imagePath == null && !File.Exists(_imagePath))
            {
                if (Firmware == "Klipper") return "avares://PrintBuddy3D/Assets/klipper-logo.png";
                if (Firmware == "Marlin") return "avares://PrintBuddy3D/Assets/marlin-outrun-nf-500.png";
                return "avares://PrintBuddy3D/Assets/other-printer-logo.png";
            }
            return _imagePath;
        }
        set
        {
            if (_imagePath != value)
            {
                _imagePath = value;
                OnPropertyChanged();
            }
        }
    }

    public Bitmap? Image {
        get
        {
            if (ImagePath != null && ImagePath.StartsWith("avares://"))
            {
                var uri = new Uri(ImagePath);
                using var stream = AssetLoader.Open(uri);
                return new Bitmap(stream);
            }
            if (File.Exists(ImagePath)) return new Bitmap(ImagePath);
            return null;
        }
    }

    private ObservableCollection<FilamentModel> _filament;
    public ObservableCollection<FilamentModel> Filament
    {
        get => _filament;
        set
        {
            if (_filament != value)
            {
                _filament = value;
                OnPropertyChanged();
            }
        }
    }
    
    
    
    public string? Status { get; set; } = "Ready"; // Status of the printer, e.g., "Ready", "Printing", "Offline", "Not Ready", "Done"
    public int E0Temp { get; set; } = 30;
    public int BedTemp { get; set; } = 30;
    public int Speed { get; set; } = 500;

}