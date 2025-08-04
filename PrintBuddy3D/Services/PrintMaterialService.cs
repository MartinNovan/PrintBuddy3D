using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using PrintBuddy3D.Models;

namespace PrintBuddy3D.Services;

public class PrintMaterialService
{
    public static PrintMaterialService Instance { get; } = new();
    
    public Task<ObservableCollection<Filament>> GetFilamentsAsync()
    {
        var filaments = new ObservableCollection<Filament>();
        using var connection = new SqliteConnection($"Data Source={AppDataService.Instance.DbPath}");
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Filaments";
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var filament = new Filament
            {
                Id = reader.GetGuid("Id"),
                DbHash = reader.GetInt32("Hash"),
                Manufacture = reader.GetString("Manufacture"),
                Name = reader.GetString("Name"),
                Color = reader.GetString("Color"),
                Weight = reader.GetInt32("Weight"),
                Price = reader.GetDouble("Price"),
                SpoolWeight = reader.GetInt32("SpoolWeight"),
                Diameter = reader.GetDouble("Diameter"),
                Density = reader.GetDouble("Density")
            };
            // Update the database with changes to the filament
            filament.PropertyChanged += (_, _) =>
            {
                if (filament.Hash != filament.DbHash)
                {
                    EditFilamentsAsync(filament);
                    filament.DbHash = filament.Hash;
                }
            };
            filaments.Add(filament);
        }
        return Task.FromResult(filaments);
    }
    public void EditFilamentsAsync(Filament filament)
    
    {
        try
        {
            using var connection = new SqliteConnection($"Data Source={AppDataService.Instance.DbPath}");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText =
                "INSERT OR REPLACE INTO Filaments (Id, Hash, Manufacture, Name, Color, Weight, Price, SpoolWeight, Diameter, Density) " +
                "VALUES (@id, @hash, @manufacture, @name, @color, @weight, @price, @spoolWeight, @diameter, @density)";
            command.Parameters.AddWithValue("@id", filament.Id);
            command.Parameters.AddWithValue("@hash", filament.Hash);
            command.Parameters.AddWithValue("@manufacture", filament.Manufacture);
            command.Parameters.AddWithValue("@name", filament.Name);
            command.Parameters.AddWithValue("@color", filament.Color);
            command.Parameters.AddWithValue("@weight", filament.Weight);
            command.Parameters.AddWithValue("@price", filament.Price);
            command.Parameters.AddWithValue("@spoolWeight", filament.SpoolWeight);
            command.Parameters.AddWithValue("@diameter", filament.Diameter);
            command.Parameters.AddWithValue("@density", filament.Density);

            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error while editing filament: " + ex.Message);
        }
    }

    public Task RemoveFilamentAsync(Filament filament)
    {
        using var connection = new SqliteConnection($"Data Source={AppDataService.Instance.DbPath}");
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Filaments WHERE Id = $id";
        command.Parameters.AddWithValue("$id", filament.Id);
        command.ExecuteNonQuery();
        
        return Task.CompletedTask;
    }
    
    public Task<ObservableCollection<Resin>> GetResinsAsync()
    {
        var resins = new ObservableCollection<Resin>();
        using var connection = new SqliteConnection($"Data Source={AppDataService.Instance.DbPath}");
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Resins";
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var resin = new Resin()
            {
                Id = reader.GetGuid("Id"),
                DbHash = reader.GetInt32("Hash"),
                Manufacture = reader.GetString("Manufacture"),
                Name = reader.GetString("Name"),
                Color = reader.GetString("Color"),
                Weight = reader.GetInt32("Weight"),
                Price = reader.GetDouble("Price")
            };
            // Update the database with changes to the resin
            resin.PropertyChanged += (_, _) =>
            {
                if (resin.Hash != resin.DbHash)
                {
                    EditResinsAsync(resin);
                    resin.DbHash = resin.Hash;
                }
            };
            resins.Add(resin);
        }
        return Task.FromResult(resins);
    }
    public void EditResinsAsync(Resin resin)
    {
        try
        {
            using var connection = new SqliteConnection($"Data Source={AppDataService.Instance.DbPath}");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText =
                "INSERT OR REPLACE INTO Resins (Id, Hash, Manufacture, Name, Color, Weight, Price) " +
                "VALUES (@id, @hash, @manufacture, @name, @color, @weight, @price)";
            command.Parameters.AddWithValue("@id", resin.Id);
            command.Parameters.AddWithValue("@hash", resin.Hash);
            command.Parameters.AddWithValue("@manufacture", resin.Manufacture);
            command.Parameters.AddWithValue("@name", resin.Name);
            command.Parameters.AddWithValue("@color", resin.Color);
            command.Parameters.AddWithValue("@weight", resin.Weight);
            command.Parameters.AddWithValue("@price", resin.Price);

            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error while editing resin: " + ex.Message);
        }
    }

    public Task RemoveResinAsync(Resin resin)
    {
        using var connection = new SqliteConnection($"Data Source={AppDataService.Instance.DbPath}");
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Resins WHERE Id = $id";
        command.Parameters.AddWithValue("$id", resin.Id);
        command.ExecuteNonQuery();
        
        return Task.CompletedTask;
    }
}