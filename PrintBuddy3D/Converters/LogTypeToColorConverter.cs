using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using PrintBuddy3D.Enums;

namespace PrintBuddy3D.Converters;

public class ConsoleLogTypeToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ConsoleLogType type)
        {
            return type switch
            {
                ConsoleLogType.Error => Brushes.Red,
                ConsoleLogType.Warning => Brushes.Orange,
                ConsoleLogType.Info => Brushes.Gray, // Nebo LightGray pro tmavý režim
                ConsoleLogType.Command => Brushes.CornflowerBlue, // Pěkná modrá
                ConsoleLogType.Success => Brushes.LightGreen,
                _ => Brushes.White
            };
        }
        return Brushes.White;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException(); // dont need for now
    }
}