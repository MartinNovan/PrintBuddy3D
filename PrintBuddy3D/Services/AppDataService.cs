using System;
using System.IO;
using Microsoft.Data.Sqlite;
using SukiUI.Enums;

namespace PrintBuddy3D.Services;

public interface IAppDataService
{
    SqliteConnection DbConnection { get; }
    void SaveConfigValue(string key, string value);
    SukiBackgroundStyle LoadBackground(string key);
    SukiColor LoadTheme(string key);
}

public class AppDataService : IAppDataService
{
    private const string FileName = "appdata.db";
    private const string BaseFolderName = "MartinNovan/PrintBuddy3D";

    public SqliteConnection DbConnection { get;}

    public AppDataService()
    {
        string basePath = GetBasePath();
        string fullFolderPath = Path.Combine(basePath, BaseFolderName);

        if (!Directory.Exists(fullFolderPath))
        {
            Directory.CreateDirectory(fullFolderPath);
        }

        var dbPath = Path.Combine(fullFolderPath, FileName);
        DbConnection = new SqliteConnection($"Data Source={dbPath}");
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
        DbConnection.Open();
        try
        {
            using var command = DbConnection.CreateCommand();
            command.CommandText =
            """
            CREATE TABLE IF NOT EXISTS Config (
                Key TEXT PRIMARY KEY,
                Value TEXT
            );
            
            INSERT OR IGNORE INTO Config (Key, Value) VALUES ("Theme","Blue");
            INSERT OR IGNORE INTO Config (Key, Value) VALUES ("Background","Gradient Soft");
                   
            CREATE TABLE IF NOT EXISTS Printers (
                Id GUID PRIMARY KEY,
                Name TEXT,
                Firmware TEXT,
                ConnectionType TEXT,
                Address TEXT,
                SerailPort TEXT,
                BaudRate INTEGER,
                SerialNumber TEXT,
                ImagePath TEXT
            );

            CREATE TABLE IF NOT EXISTS Filaments (
                Id GUID PRIMARY KEY,
                Hash INTEGER,
                Manufacture TEXT,
                Name TEXT,
                Firmware TEXT,
                ConnectionType INTEGER,
                Price DOUBLE,
                Diameter DOUBLE,
                Density DOUBLE,
                SpoolWeight INTEGER
            );

            CREATE TABLE IF NOT EXISTS Resins (
                Id GUID PRIMARY KEY,
                Hash INTEGER,
                Manufacture TEXT,
                Name TEXT,
                Firmware TEXT,
                ConnectionType INTEGER,
                Price DOUBLE
            );
            """;
            command.ExecuteNonQuery();
        }
        finally
        {
            DbConnection.Close();
        }
    }

    public void SaveConfigValue(string key, string value)
    {
        DbConnection.Open();
        try
        {
            using var command = DbConnection.CreateCommand();
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
        finally
        {
            DbConnection.Close();
        }
    }

    public SukiBackgroundStyle LoadBackground(string key)
    {
        DbConnection.Open();
        try
        {
            using var command = DbConnection.CreateCommand();
            command.CommandText = "SELECT Value FROM Config WHERE Key = $key LIMIT 1";
            command.Parameters.AddWithValue("$key", key);
            var result = command.ExecuteScalar();
            return result is string s && Enum.TryParse(s, out SukiBackgroundStyle style)
                ? style : SukiBackgroundStyle.GradientSoft;
        }
        finally
        {
            DbConnection.Close();
        }
    }
    public SukiColor LoadTheme(string key)
    {
        DbConnection.Open();
        try
        {
            using var command = DbConnection.CreateCommand();
            command.CommandText = "SELECT Value FROM Config WHERE Key = $key LIMIT 1";
            command.Parameters.AddWithValue("$key", key);
            var result = command.ExecuteScalar();
            return result is string s && Enum.TryParse(s, out SukiColor style)
                ? style : SukiColor.Blue;
        }
        finally
        {
            DbConnection.Close();
        }
    }
}
