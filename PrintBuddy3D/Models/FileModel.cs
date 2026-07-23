using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PrintBuddy3D.Models;

public partial class FileModel : ObservableObject
{
    [ObservableProperty] private string _fileName = "";
    [ObservableProperty] private string _filePath = "";
    [ObservableProperty] private long _fileSize;
    [ObservableProperty] private DateTime _lastModified;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(HasThumbNail))] private string? _thumbnailPath;
    public bool HasThumbNail => !string.IsNullOrEmpty(ThumbnailPath);
    public bool IsFileGcode => IsGcode(FileName); // for showing thumbnail if it has one
    public bool IsFileProject => IsProject(FileName); // for showing thumbnail if it has one
    public string FileSizeFormatted => FormatSize(FileSize);

    private static bool IsProject(string fileName)
    {
        // if file name ends with .3mf returns true (app doesnt check if its really project, only checks the file extension)
        return fileName.ToLower().EndsWith(".3mf");
    }

    private static bool IsGcode(string fileName)
    {
        // if filename ends with .gcode or .bgcode return true 
        return fileName.ToLower().EndsWith(".gcode") || fileName.ToLower().EndsWith(".bgcode");
    }
    
    // Method for formating size to show the best suffix
    private static string FormatSize(long bytes)
    {
        string[] suffixes = ["B", "KB", "MB", "GB", "TB"];
        var counter = 0;
        decimal number = bytes;
        while (Math.Round(number / 1024) >= 1)
        {
            number /= 1024;
            counter++;
        }
        return $"{number:n1} {suffixes[counter]}";
    }
}