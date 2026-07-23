using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PrintBuddy3D.Models;

namespace PrintBuddy3D.Services;

public interface IFilesService
{
    // Load files from directory with extension that is in allowedExtensions, returns list of filemodel
    Task<List<FileModel>> GetFilesInDirectoryAsync(string directoryPath, string[] allowedExtensions);
    // Method for uploading Gcode file to multiple printers, returns list of results
    Task<List<(PrinterModel Printer, bool IsSuccess, string ErrorMessage)>> UploadToPrintersAsync(string filePath, IEnumerable<PrinterModel> targetPrinters, bool startPrint);
    // Method for extracting thumbnail from start of the gcode file
    public Task<string?> ExtractThumbnailFromGcodeAsync(string gcodePath, string cacheDirectory);
    // Method for extracting thumbnail from 3mf project files (unzips the 3mf file as normal zip file and finds the image in subdirectory)
    public Task<string?> ExtractThumbnailFromProjectAsync(string threeMfPath, string cacheDirectory);
}

public class FilesService(IPrinterControlServiceFactory printerControlServiceFactory) : IFilesService
{
    public async Task<List<FileModel>> GetFilesInDirectoryAsync(string directoryPath, string[] allowedExtensions)
    {
        return await Task.Run(() =>
        {
            // get dir info
            var directoryInfo = new DirectoryInfo(directoryPath);
            // if dir not exists return empty list
            if (!directoryInfo.Exists) return new List<FileModel>();

            // Search all dirrectories, get files with the extension we are looking for, and return it as list
            return directoryInfo.GetFiles("*.*", SearchOption.AllDirectories)
                .Where(f => allowedExtensions.Contains(f.Extension.ToLower()))
                .Select(f => new FileModel
                {
                    FileName = f.Name,
                    FilePath = f.FullName,
                    FileSize = f.Length,
                    LastModified = f.LastWriteTime
                }).ToList();
        });
    }
    
    public async Task<List<(PrinterModel Printer, bool IsSuccess, string ErrorMessage)>> UploadToPrintersAsync(string filePath, IEnumerable<PrinterModel> targetPrinters, bool startPrint)
    {
        var uploadTasks = targetPrinters.Select(async printer =>
        {
            // Get the correct PrinterControlService from the factory
            var controlService = printerControlServiceFactory.Create(printer);
            // Pass the parameters to the method in the PrinterControlService and get result 
            var result = await controlService.UploadGcodeAsync(printer, filePath, startPrint);
            return (Printer: printer, result.IsSuccess, result.ErrorMessage);
        });
        var results = await Task.WhenAll(uploadTasks); // Wait for all tasks to be finished and store them
        return results.ToList(); // return the results as list
    }
    
    public async Task<string?> ExtractThumbnailFromGcodeAsync(string gcodePath, string cacheDirectory)
    {
        try
        {
            // Get the file info
            var fileInfo = new FileInfo(gcodePath);
            // Create unique cache name (name + size)
            var cacheFileName = $"gcode_{fileInfo.Name}_{fileInfo.Length}.png";
            var cacheFilePath = Path.Combine(cacheDirectory, cacheFileName);

            // If already exists in cache, return that
            if (File.Exists(cacheFilePath))
                return cacheFilePath;

            // Create dir if not exists
            if (!Directory.Exists(cacheDirectory))
                Directory.CreateDirectory(cacheDirectory);

            // read the file
            using var reader = new StreamReader(gcodePath);
            var inThumbnail = false;
            
            var currentBuilder = new StringBuilder();
            string? bestBase64 = null;
            var linesRead = 0;
            
            // Try to find the thumbnail in first 8k lines or continue if we are in the thumbnail reading
            while (await reader.ReadLineAsync() is { } line && (linesRead < 8000 || inThumbnail))
            {
                linesRead++;
                
                // Found the start of thumbnail
                if (line.StartsWith("; thumbnail begin"))
                {
                    inThumbnail = true; // We start to collect data
                    currentBuilder.Clear(); // Clear currentBuilder for higher resolution (slicer first do the smallest resolution and go up in resolution)
                    continue; 
                }
                
                // if we didnt found the beginnig, continue searching for the thumbnail 
                if (!inThumbnail) continue;
                
                // If we found the end
                if (line.StartsWith("; thumbnail end"))
                {
                    inThumbnail = false; // stop reading data
                        
                    // If the newly foung thumbnail is larger than the one we already saved, overwrite it
                    if (bestBase64 == null || currentBuilder.Length > bestBase64.Length)
                    {
                        bestBase64 = currentBuilder.ToString();
                    }
                        
                    continue; // Search for even larger thumbnail, than we currently have 
                }
                    
                // Save the data of the thumbnail
                if (line.Length > 2)
                {
                    currentBuilder.Append(line.Substring(2).Trim());
                }
            }

            // If we found atleast one, save it to cache for future
            if (!string.IsNullOrEmpty(bestBase64))
            {
                var imageBytes = Convert.FromBase64String(bestBase64);
                await File.WriteAllBytesAsync(cacheFilePath, imageBytes);
                return cacheFilePath;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error extracting the gcode thumbnail: {ex.Message}");
        }

        return null; // No thumbnail found, return null
    }
    
    public async Task<string?> ExtractThumbnailFromProjectAsync(string threeMfPath, string cacheDirectory)
    {
        try
        {
            // Get file info
            var fileInfo = new FileInfo(threeMfPath);
            // Create unique name for file
            var cacheFileName = $"3mf_{fileInfo.Name}_{fileInfo.Length}.png";
            var cacheFilePath = Path.Combine(cacheDirectory, cacheFileName);

            // return cached thumbnail if exists
            if (File.Exists(cacheFilePath))
                return cacheFilePath;

            // Create dir if not exists
            if (!Directory.Exists(cacheDirectory))
                Directory.CreateDirectory(cacheDirectory);

            // Unzip the file in background
            await Task.Run(() =>
            {
                using ZipArchive archive = ZipFile.OpenRead(threeMfPath);
                // Searching in for image/thumbnail in the unzipped 3mf file
                var thumbnailEntries = archive.Entries
                    .Where(e => e.FullName.Contains("thumbnail", StringComparison.OrdinalIgnoreCase) && e.FullName.EndsWith(".png", StringComparison.OrdinalIgnoreCase)).ToList();

                // if not found anything then return
                if (!thumbnailEntries.Any())
                    return;

                // Find the biggest image file (should be the best resolution)
                var largestThumbnail = thumbnailEntries.OrderByDescending(e => e.Length).First();

                // Save to file
                largestThumbnail.ExtractToFile(cacheFilePath, overwrite: true);
            });
            
            if (File.Exists(cacheFilePath))
                return cacheFilePath; // return the cached thumbnail
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error extracting the 3mf thumbnail: {ex.Message}");
        }
        
        // if not found anything, return null
        return null;
    }
}