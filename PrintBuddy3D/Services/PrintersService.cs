using System.Collections.ObjectModel;
using Microsoft.Data.Sqlite;
using PrintBuddy3D.Models;

namespace PrintBuddy3D.Services;

public interface IPrintersService
{
    
}

public class PrintersService(IAppDataService appDataService) : IPrintersService
{
    private readonly SqliteConnection _dbConnection = appDataService.DbConnection;
    
    
}