using System;
using System.IO;
using Microsoft.Data.Sqlite;
using SukiUI;
using SukiUI.Enums;
using SukiUI.Models;

namespace PrintBuddy3D.Services;

public class AppDataService
{
    public static AppDataService Instance { get; } = new();
    
    private const string FileName = "appdata.db";
    private const string BaseFolderName = "MartinNovan/PrintBuddy3D";

    private string? _dbPath;

    public AppDataService()
    {
        string basePath = GetBasePath();
        string fullFolderPath = Path.Combine(basePath, BaseFolderName);

        if (!Directory.Exists(fullFolderPath))
        {
            Directory.CreateDirectory(fullFolderPath);
        }

        _dbPath = Path.Combine(fullFolderPath, FileName);

        if (!File.Exists(_dbPath))
        {
            InitializeDatabase();
        }
    }

    private string GetBasePath()
    {
        if (OperatingSystem.IsWindows())
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        if (OperatingSystem.IsLinux())
            return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            
        if (OperatingSystem.IsAndroid())
            return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        if (OperatingSystem.IsIOS())
            return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        throw new PlatformNotSupportedException("Supported platforms are: Windows, Android, and iOS.");
    }

    private void InitializeDatabase()
    {
        using var connection = new SqliteConnection($"Data Source={_dbPath}");
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText =
        """
        CREATE TABLE IF NOT EXISTS Config (
            Key TEXT PRIMARY KEY,
            Value TEXT
        );
        
        INSERT OR IGNORE INTO Config (Key, Value) VALUES ("Theme","Blue");
        INSERT OR IGNORE INTO Config (Key, Value) VALUES ("Background","Gradient Soft");
        """;

        command.ExecuteNonQuery();
    }

    public void SaveConfigValue(string key, string value)
    {
        using var connection = new SqliteConnection($"Data Source={_dbPath}");
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText =
        """
        INSERT INTO Config (Key, Value)
        VALUES ($key, $value)
        ON CONFLICT(Key) DO UPDATE SET Value = excluded.Value;
        """;
        command.Parameters.AddWithValue("$key", key);
        command.Parameters.AddWithValue("$value", value);
        command.ExecuteNonQuery();
    }

    public SukiBackgroundStyle LoadBackground(string key)
    {
        using var connection = new SqliteConnection($"Data Source={_dbPath}");
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT Value FROM Config WHERE Key = $key LIMIT 1";
        command.Parameters.AddWithValue("$key", key);

        var result = command.ExecuteScalar();
        return result switch
        {
            null => SukiBackgroundStyle.GradientSoft, // Default value if not found
            string value => Enum.TryParse(value, out SukiBackgroundStyle style) ? style : SukiBackgroundStyle.GradientSoft,
            _ => SukiBackgroundStyle.GradientSoft // Fallback for unexpected types
        };
    }
    public SukiColor LoadTheme(string key)
    {
        using var connection = new SqliteConnection($"Data Source={_dbPath}");
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT Value FROM Config WHERE Key = $key LIMIT 1";
        command.Parameters.AddWithValue("$key", key);

        var result = command.ExecuteScalar();
        return result switch
        {
            null => SukiColor.Blue, // Default value if not found
            string value => Enum.TryParse(value, out SukiColor style) ? style : SukiColor.Blue,
            _ => SukiColor.Blue // Fallback for unexpected types
        };
    }
}
