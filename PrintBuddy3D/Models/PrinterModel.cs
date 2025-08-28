using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using PrintBuddy3D.Enums;

namespace PrintBuddy3D.Models;

public sealed class PrinterModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    public Guid Id { get; init; } = Guid.NewGuid(); // Unique identifier for each printer
    
    public int DbHash { get; set; } // Hash for database tracking, used to detect changes easily
    
    public int Hash // Computed hash for change detection
    {
        get
        {
            var hash = new HashCode();
            hash.Add(Id);
            hash.Add(Name);
            hash.Add(Firmware.ToString());
            hash.Add(Prefix.ToString());
            hash.Add(Address);
            hash.Add(HostUserName);
            hash.Add(LastSerialPort);
            hash.Add(BaudRate);
            hash.Add(SerialNumber);
            hash.Add(ImagePath);
            hash.Add(IsMultiFilament);
            return hash.ToHashCode();
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

    private PrinterEnums.Firmware? _firmware;
    public PrinterEnums.Firmware? Firmware
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
    
    public string MarlinFirmwareUuid { get; set; } // Unique identifier for Marlin firmware. Used to distinguish Marlin printers with the same serial port. Can be found using M115. Will be assigned when the printer is connected for the first time.
    
    private PrinterEnums.Prefix? _prefix;
    public PrinterEnums.Prefix? Prefix // e.g., "http://" or "https://", is used for webview access
    {
        get => _prefix;
        set
        {
            if (_prefix != value)
            {
                _prefix = value;
                OnPropertyChanged();
                OnPropertyChanged(FullAddress);
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
                _address = value?.Replace("http://", "").Replace("https://", ""); // Ensure no prefix is stored here, it's handled by Prefix property
                OnPropertyChanged();
                OnPropertyChanged(FullAddress);
            }
        }
    }
    
    public string? FullAddress
    {
        get
        {
            if (Firmware == PrinterEnums.Firmware.Marlin) return null; // Marlin printers don't use web access
            return $"{Prefix}://{Address}";
            // Full address with prefix, e.g., "http://, used for webview access
        }
    }

    private string? _hostUserName;
    public string? HostUserName
    {
        get => _hostUserName;
        set
        {
            if (_hostUserName != value)
            {
                _hostUserName = value;
                OnPropertyChanged();
            }
        }
    }
    
    private string? _lastSerialPort;
    public string? LastSerialPort // e.g., "COM3" or "/dev/ttyUSB0", is used for quicker access to the printer, if you don't swap USB ports
    {
        get => _lastSerialPort;
        set
        {
            if (_lastSerialPort != value)
            {
                _lastSerialPort = value;
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
                if (Firmware == PrinterEnums.Firmware.Klipper) return "avares://PrintBuddy3D/Assets/klipper-logo.png";
                if (Firmware == PrinterEnums.Firmware.Marlin) return "avares://PrintBuddy3D/Assets/marlin-outrun-nf-500.png";
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

    private bool IsMultiFilament { get; set; } // Whether the printer supports multi-filament, e.g., 2 or more extruders

    public string? PreviousStatus { get; private set; } // Previous status of the printer, used to track changes
    
    private string? _status = "None"; // Default status is Offline, can be changed to Online, Printing, Done, Idle, etc.

    public string? Status
    {
        get => _status;
        set
        {
            if (_status != value)
            {
                PreviousStatus = _status;
                _status = value;
                OnPropertyChanged();
            }
        }
    } // Status of the printer, e.g., "Online", "Printing", "Offline", "Done", "Idle"
    
    public int E0Temp { get; set; } = 30;
    public int BedTemp { get; set; } = 30;
    public int Speed { get; set; } = 500;
    public object? CurrentJob { get; set; } // Current job being printed, can be a string or a more complex object
}