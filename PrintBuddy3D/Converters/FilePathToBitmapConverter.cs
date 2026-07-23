using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;

namespace PrintBuddy3D.Converters;

public class FilePathToBitmapConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string path || !System.IO.File.Exists(path)) return null;
        
        try
        {
            return new Bitmap(path); // Load the picture from path
        }
        catch
        {
            return null; 
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null; // Not needed 
    }
}
