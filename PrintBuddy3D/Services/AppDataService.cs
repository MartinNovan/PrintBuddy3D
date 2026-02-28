using System;
using System.IO;
using Microsoft.Data.Sqlite;
using SukiUI.Enums;

namespace PrintBuddy3D.Services;

public interface IAppDataService
{
    string ConnectionString { get; }
    void SaveConfigValue(string key, string value);
    SukiBackgroundStyle LoadBackground(string key);
    SukiColor LoadTheme(string key);
}

public class AppDataService : IAppDataService
{
    private const string FileName = "appdata.db";
    private const string BaseFolderName = "MartinNovan/PrintBuddy3D";

    public string ConnectionString { get; } 

    public AppDataService()
    {
        string basePath = GetBasePath();
        string fullFolderPath = Path.Combine(basePath, BaseFolderName);

        if (!Directory.Exists(fullFolderPath))
            Directory.CreateDirectory(fullFolderPath);

        var dbPath = Path.Combine(fullFolderPath, FileName);
        ConnectionString = $"Data Source={dbPath}";
        InitializeDatabase();
    }

    private string GetBasePath()
    {
        var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrEmpty(path))
            throw new PlatformNotSupportedException("Unsupported platform.");
        return path;
    }

    private void InitializeDatabase()
    {
        using var connection = new SqliteConnection(ConnectionString); 
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
                              CREATE TABLE IF NOT EXISTS Config (
                                  Key TEXT PRIMARY KEY,
                                  Value TEXT
                              );
                              INSERT OR IGNORE INTO Config (Key, Value) VALUES ("Theme","Blue");
                              INSERT OR IGNORE INTO Config (Key, Value) VALUES ("Background","Gradient Soft");
                                     
                              CREATE TABLE IF NOT EXISTS Printers (
                                  Id GUID PRIMARY KEY,
                                  Hash INTEGER,
                                  Name TEXT,
                                  Firmware INTEGER,
                                  Prefix INTEGER,
                                  Address TEXT,
                                  HostUserName TEXT,
                                  LastSerialPort TEXT,
                                  BaudRate INTEGER,
                                  SerialNumber TEXT,
                                  ImagePath TEXT
                              );

                              CREATE TABLE IF NOT EXISTS Filaments (
                                  Id GUID PRIMARY KEY,
                                  Hash INTEGER,
                                  Manufacture TEXT,
                                  Name TEXT,
                                  Color TEXT,
                                  Weight INTEGER,
                                  Price DOUBLE,
                                  Diameter DOUBLE,
                                  Density DOUBLE,
                                  SpoolWeight INTEGER
                              );
                              """;
        command.ExecuteNonQuery();
    }

    public void SaveConfigValue(string key, string value)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
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
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Value FROM Config WHERE Key = $key LIMIT 1";
        command.Parameters.AddWithValue("$key", key);
        var result = command.ExecuteScalar();
        return result is string s && Enum.TryParse(s, out SukiBackgroundStyle style)
            ? style : SukiBackgroundStyle.GradientSoft;
    }

    public SukiColor LoadTheme(string key)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Value FROM Config WHERE Key = $key LIMIT 1";
        command.Parameters.AddWithValue("$key", key);
        var result = command.ExecuteScalar();
        return result is string s && Enum.TryParse(s, out SukiColor style)
            ? style : SukiColor.Blue;
    }
}