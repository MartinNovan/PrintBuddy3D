using System.Collections.ObjectModel;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using PrintBuddy3D.Models;

namespace PrintBuddy3D.Services;

public interface IPrintMaterialService
{
    Task<ObservableCollection<FilamentModel>> GetFilamentsAsync(CancellationToken ct = default);
    Task UpsertFilamentAsync(FilamentModel filamentModel, CancellationToken ct = default);
    Task RemoveFilamentAsync(FilamentModel filamentModel, CancellationToken ct = default);
}

public class PrintMaterialService(IAppDataService appDataService) : IPrintMaterialService
{
    private readonly string _connectionString = appDataService.ConnectionString;

    public async Task<ObservableCollection<FilamentModel>> GetFilamentsAsync(CancellationToken ct = default)
    {
        var filaments = new ObservableCollection<FilamentModel>();
        await using var connection = new SqliteConnection(_connectionString); 
        await connection.OpenAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Filaments";

        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var filament = new FilamentModel
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

    public async Task UpsertFilamentAsync(FilamentModel filamentModel, CancellationToken ct = default)
    {
        await using var connection = new SqliteConnection(_connectionString); 
        await connection.OpenAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Filaments (Id, Hash, Manufacture, Name, Color, Weight, Price, SpoolWeight, Diameter, Density)
            VALUES ($id, $hash, $manufacture, $name, $color, $weight, $price, $spoolWeight, $diameter, $density)
            ON CONFLICT(Id) DO UPDATE SET
                Id = excluded.Id,
                Hash = excluded.Hash,
                Manufacture = excluded.Manufacture,
                Name = excluded.Name,
                Color = excluded.Color,
                Weight = excluded.Weight,
                Price = excluded.Price,
                SpoolWeight = excluded.SpoolWeight,
                Diameter = excluded.Diameter,
                Density = excluded.Density;";
        command.Parameters.AddWithValue("$id", filamentModel.Id);
        command.Parameters.AddWithValue("$hash", filamentModel.Hash);
        command.Parameters.AddWithValue("$manufacture", filamentModel.Manufacture);
        command.Parameters.AddWithValue("$name", filamentModel.Name);
        command.Parameters.AddWithValue("$color", filamentModel.Color);
        command.Parameters.AddWithValue("$weight", filamentModel.Weight);
        command.Parameters.AddWithValue("$price", filamentModel.Price);
        command.Parameters.AddWithValue("$spoolWeight", filamentModel.SpoolWeight);
        command.Parameters.AddWithValue("$diameter", filamentModel.Diameter);
        command.Parameters.AddWithValue("$density", filamentModel.Density);

        await command.ExecuteNonQueryAsync(ct);
    }

    public async Task RemoveFilamentAsync(FilamentModel filamentModel, CancellationToken ct = default)
    {
        await using var connection = new SqliteConnection(_connectionString); 
        await connection.OpenAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Filaments WHERE Id = $id";
        command.Parameters.AddWithValue("$id", filamentModel.Id);
        await command.ExecuteNonQueryAsync(ct);
    }
}