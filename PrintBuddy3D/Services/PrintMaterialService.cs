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
    
    Task<ObservableCollection<ResinModel>> GetResinsAsync(CancellationToken ct = default);
    Task UpsertResinAsync(ResinModel resinModel, CancellationToken ct = default);
    Task RemoveResinAsync(ResinModel resinModel, CancellationToken ct = default);
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
    
    public async Task<ObservableCollection<ResinModel>> GetResinsAsync(CancellationToken ct = default)
    {
        var resins = new ObservableCollection<ResinModel>();
        await _dbConnection.OpenAsync(ct);

        await using var command = _dbConnection.CreateCommand();
        command.CommandText = "SELECT * FROM Resins";
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var resin = new ResinModel()
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
    public async Task UpsertResinAsync(ResinModel resinModel, CancellationToken ct = default)
    {
        await _dbConnection.OpenAsync(ct);

        await using var command = _dbConnection.CreateCommand();
        command.CommandText = @"INSERT INTO Resins (Id, Hash, Manufacture, Name, Color, Weight, Price)
                                VALUES ($id, $hash, $manufacture, $name, $color, $weight, $price)
                                ON CONFLICT(Id) DO UPDATE SET
                                    Hash = excluded.Hash,
                                    Manufacture = excluded.Manufacture,
                                    Name = excluded.Name,
                                    Color = excluded.Color,
                                    Weight = excluded.Weight,
                                    Price = excluded.Price;";
        command.Parameters.AddWithValue("$id", resinModel.Id);
        command.Parameters.AddWithValue("$hash", resinModel.Hash);
        command.Parameters.AddWithValue("$manufacture", resinModel.Manufacture);
        command.Parameters.AddWithValue("$name", resinModel.Name);
        command.Parameters.AddWithValue("$color", resinModel.Color);
        command.Parameters.AddWithValue("$weight", resinModel.Weight);
        command.Parameters.AddWithValue("$price", resinModel.Price);

        await command.ExecuteNonQueryAsync(ct);
    }

    public async Task RemoveResinAsync(ResinModel resinModel, CancellationToken ct = default)
    {
        await _dbConnection.OpenAsync(ct);

        await using var command = _dbConnection.CreateCommand();
        command.CommandText = "DELETE FROM Resins WHERE Id = $id";
        command.Parameters.AddWithValue("$id", resinModel.Id);
        await command.ExecuteNonQueryAsync(ct);
    }
}