using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

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
            if (_connectionType == value)
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
        get => _imagePath;
        set
        {
            if (_imagePath != value)
            {
                _imagePath = value;
                OnPropertyChanged();
            }
        }
    }
    
    private string? Status { get; set; } = "Ready"; // Status of the printer, e.g., "Ready", "Printing", "Oflline", "Not Ready" <- not ready is for printers that use filament sensors, so they are not ready until filament is loaded
    public int E0Temp { get; set; } = 30;
    public int BedTemp { get; set; } = 30;
    public string Filament { get; set; } = "PLA";
    public int Speed { get; set; } = 500;

}