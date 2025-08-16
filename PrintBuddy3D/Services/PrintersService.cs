using Microsoft.Data.Sqlite;

namespace PrintBuddy3D.Services;

public interface IPrintersService
{
    
}

public class PrintersService(IAppDataService appDataService) : IPrintersService
{
    private readonly SqliteConnection _dbConnection = appDataService.DbConnection;
    
    
}