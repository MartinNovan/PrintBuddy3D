using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using PrintBuddy3D.Models;

namespace PrintBuddy3D.Services;

public class PrintMaterialService
{
    public static PrintMaterialService Instance { get; } = new();
    
    public async Task<ObservableCollection<Filament>> GetFilamentsAsync(CancellationToken ct = default)
    {
        var filaments = new ObservableCollection<Filament>();
        await using var connection = new SqliteConnection($"Data Source={AppDataService.Instance.DbPath}");
        await connection.OpenAsync(ct);
        
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Filaments";
        
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
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
        
            filament.PropertyChanged += async (_, _) =>
            {
                if (filament.Hash != filament.DbHash)
                {
                    await UpsertFilamentAsync(filament, ct);
                    filament.DbHash = filament.Hash;
                }
            };
        
            filaments.Add(filament);
        }
        return filaments;
    }
    public async Task UpsertFilamentAsync(Filament filament, CancellationToken ct = default)
    {
        await using var connection = new SqliteConnection($"Data Source={AppDataService.Instance.DbPath}");
        await connection.OpenAsync(ct);
    
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Filaments (Id, Hash, Manufacture, Name, Color, Weight, Price, SpoolWeight, Diameter, Density)
            VALUES ($id, $hash, $manufacture, $name, $color, $weight, $price, $spoolWeight, $diameter, $density)
            ON CONFLICT(Id) DO UPDATE SET
                Hash = excluded.Hash,
                Manufacture = excluded.Manufacture,
                Name = excluded.Name,
                Color = excluded.Color,
                Weight = excluded.Weight,
                Price = excluded.Price,
                SpoolWeight = excluded.SpoolWeight,
                Diameter = excluded.Diameter,
                Density = excluded.Density;";
        cmd.Parameters.AddWithValue("$id", filament.Id);
        cmd.Parameters.AddWithValue("$hash", filament.Hash);
        cmd.Parameters.AddWithValue("$manufacture", filament.Manufacture);
        cmd.Parameters.AddWithValue("$name", filament.Name);
        cmd.Parameters.AddWithValue("$color", filament.Color);
        cmd.Parameters.AddWithValue("$weight", filament.Weight);
        cmd.Parameters.AddWithValue("$price", filament.Price);
        cmd.Parameters.AddWithValue("$spoolWeight", filament.SpoolWeight);
        cmd.Parameters.AddWithValue("$diameter", filament.Diameter);
        cmd.Parameters.AddWithValue("$density", filament.Density);
    
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task RemoveFilamentAsync(Filament filament, CancellationToken ct = default)
    {
        await using var connection = new SqliteConnection($"Data Source={AppDataService.Instance.DbPath}");
        await connection.OpenAsync(ct);

        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Filaments WHERE Id = $id";
        command.Parameters.AddWithValue("$id", filament.Id);
        await command.ExecuteNonQueryAsync(ct);
    }
    
    public async Task<ObservableCollection<Resin>> GetResinsAsync(CancellationToken ct = default)
    {
        var resins = new ObservableCollection<Resin>();
        await using var connection = new SqliteConnection($"Data Source={AppDataService.Instance.DbPath}");
        await connection.OpenAsync(ct);

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Resins";
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
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
            resin.PropertyChanged += async (_, _) =>
            {
                if (resin.Hash != resin.DbHash)
                {
                    await UpsertResinAsync(resin, ct);
                    resin.DbHash = resin.Hash;
                }
            };
            resins.Add(resin);
        }
        return resins;
    }
    public async Task UpsertResinAsync(Resin resin, CancellationToken ct = default)
    {
        await using var connection = new SqliteConnection($"Data Source={AppDataService.Instance.DbPath}");
        await connection.OpenAsync(ct);

        await using var command = connection.CreateCommand();
        command.CommandText = @"INSERT INTO Resins (Id, Hash, Manufacture, Name, Color, Weight, Price)
                                VALUES ($id, $hash, $manufacture, $name, $color, $weight, $price)
                                ON CONFLICT(Id) DO UPDATE SET
                                    Hash = excluded.Hash,
                                    Manufacture = excluded.Manufacture,
                                    Name = excluded.Name,
                                    Color = excluded.Color,
                                    Weight = excluded.Weight,
                                    Price = excluded.Price;";
        command.Parameters.AddWithValue("$id", resin.Id);
        command.Parameters.AddWithValue("$hash", resin.Hash);
        command.Parameters.AddWithValue("$manufacture", resin.Manufacture);
        command.Parameters.AddWithValue("$name", resin.Name);
        command.Parameters.AddWithValue("$color", resin.Color);
        command.Parameters.AddWithValue("$weight", resin.Weight);
        command.Parameters.AddWithValue("$price", resin.Price);

        await command.ExecuteNonQueryAsync(ct);
    }

    public async Task RemoveResinAsync(Resin resin, CancellationToken ct = default)
    {
        await using var connection = new SqliteConnection($"Data Source={AppDataService.Instance.DbPath}");
        await connection.OpenAsync(ct);

        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Resins WHERE Id = $id";
        command.Parameters.AddWithValue("$id", resin.Id);
        await command.ExecuteNonQueryAsync(ct);
    }
}