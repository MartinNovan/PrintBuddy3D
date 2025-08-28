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
    private readonly SqliteConnection _dbConnection = appDataService.DbConnection;

    public async Task<ObservableCollection<FilamentModel>> GetFilamentsAsync(CancellationToken ct = default)
    {
        var filaments = new ObservableCollection<FilamentModel>();
        await _dbConnection.OpenAsync(ct);

        await using var command = _dbConnection.CreateCommand();
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
        await _dbConnection.OpenAsync(ct);

        await using var cmd = _dbConnection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Filaments (Id, Hash, Manufacture, Name, Color, Weight, Price, SpoolWeight, Diameter, Density)
            VALUES ($id, $hash, $manufacture, $name, $color, $weight, $price, $spoolWeight, $diameter, $density)
            ON CONFLICT(Id) DO UPDATE SET
                PrinterId = excluded.PrinterId,
                Hash = excluded.Hash,
                Manufacture = excluded.Manufacture,
                Name = excluded.Name,
                Color = excluded.Color,
                Weight = excluded.Weight,
                Price = excluded.Price,
                SpoolWeight = excluded.SpoolWeight,
                Diameter = excluded.Diameter,
                Density = excluded.Density;";
        cmd.Parameters.AddWithValue("$id", filamentModel.Id);
        cmd.Parameters.AddWithValue("$hash", filamentModel.Hash);
        cmd.Parameters.AddWithValue("$manufacture", filamentModel.Manufacture);
        cmd.Parameters.AddWithValue("$name", filamentModel.Name);
        cmd.Parameters.AddWithValue("$color", filamentModel.Color);
        cmd.Parameters.AddWithValue("$weight", filamentModel.Weight);
        cmd.Parameters.AddWithValue("$price", filamentModel.Price);
        cmd.Parameters.AddWithValue("$spoolWeight", filamentModel.SpoolWeight);
        cmd.Parameters.AddWithValue("$diameter", filamentModel.Diameter);
        cmd.Parameters.AddWithValue("$density", filamentModel.Density);

        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task RemoveFilamentAsync(FilamentModel filamentModel, CancellationToken ct = default)
    {
        await _dbConnection.OpenAsync(ct);

        await using var command = _dbConnection.CreateCommand();
        command.CommandText = "DELETE FROM Filaments WHERE Id = $id";
        command.Parameters.AddWithValue("$id", filamentModel.Id);
        await command.ExecuteNonQueryAsync(ct);
    }
}