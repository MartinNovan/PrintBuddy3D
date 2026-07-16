using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PrintBuddy3D.ViewModels.Pages; // To access MenuItemDto

namespace PrintBuddy3D.Services;

public interface IGuidesService
{
    Task<bool> SyncAllOfflineDataAsync();
    Task<(string? Content, bool IsOffline)> GetWikiFileTextAsync(string fileName);
    Task<string?> GetWikiPageForDisplayAsync(string fileName);
}

public class GuidesService : IGuidesService
{
    private static readonly HttpClient HttpWikiClient = new() { Timeout = TimeSpan.FromSeconds(5) }; // Shared wiki http client
    private const string BaseWikiUrl = "https://raw.githubusercontent.com/wiki/MartinNovan/PrintBuddy3D/"; // url where the wiki is (probably gonna add to settings for easy override)
    private readonly string _cacheDirectory;

    public GuidesService(IAppDataService appDataService)
    {
        // create cache directory if not exist
        _cacheDirectory = Path.Combine(appDataService.GetAppBasePath(), "Cache", "Wiki");
        if (!Directory.Exists(_cacheDirectory)) Directory.CreateDirectory(_cacheDirectory);
        
        // create folder for assets if not exist
        string assetsDir = Path.Combine(_cacheDirectory, "_Assets");
        if (!Directory.Exists(assetsDir)) Directory.CreateDirectory(assetsDir);
    }

    public async Task<bool> SyncAllOfflineDataAsync()
    {
        try
        {
            // Get the _Menu.json (if newer on wiki, get _Menu.json from wiki, else load _Menu.json from cache)
            var (menuJson, isOffline) = await GetWikiFileTextAsync("_Menu.json");
        
            // If the previous method failed/cannot connect/cache doesn't exist return false
            if (string.IsNullOrEmpty(menuJson) || isOffline) return false;
            
            // If internet works and menuJson is not null, recursively update every file listed in the _Menu.json
            var items = JsonSerializer.Deserialize<List<MenuItemDto>>(menuJson); // Get names from menuJson
            if (items == null) return false; // if somehow items are null, return false

            // Now recursively check every item for update on remote
            await SyncMenuStructureRecursively(items);
            return true; // return true as indicating all files were checked and updated
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GuidesService] Error when trying to sync wiki local cache with remote: {ex.Message}");
            return false;
        }
    }

    private async Task SyncMenuStructureRecursively(List<MenuItemDto> items)
    {
        // Run thru every item
        foreach (var item in items)
        {
            if (!string.IsNullOrEmpty(item.Page))
            {
                // Get the full file name with the end ".md"
                string fileName = $"{item.Page}.md";
                // Get the updated or local file 
                var result = await GetWikiFileTextAsync(fileName);
                
                // TODO check if the internet connection is still up (with attribute IsOffline) and handle this edge case
                
                // If file has some content (which it should)
                if (!string.IsNullOrEmpty(result.Content))
                {
                    await CacheAssetsFromMarkdownAsync(result.Content); // Check for assets links in file and if there is, download them to cache.
                }
            }

            // Recursion for subpages
            if (item.Children is { Count: > 0 })
            {
                await SyncMenuStructureRecursively(item.Children); // do the same thing for subpages
            }
        }
    }

    /// <summary>
    /// Method gets file name and automatically checks local cache and compare local etag with remote etag, to see if any changes were made to the file.
    /// If network works and changes were made, it returns Content = {content of file from remote} with IsOffline = 0. (local cache is automatically updated)
    /// If network fails / network works and changes weren't made at remote, it returns Content = {content of file in local cache} with IsOffline = {1 if network fails, 0 if network works but no changes were made}.
    /// If network fails and file is not in the cache folder, it returns Content = null with IsOffline = 1.
    /// </summary>
    public async Task<(string? Content, bool IsOffline)> GetWikiFileTextAsync(string fileName)
    {
        string localFilePath = Path.Combine(_cacheDirectory, fileName.Replace('/', Path.DirectorySeparatorChar)); // Get the local path for the file
        string etagFilePath = localFilePath + ".etag"; // Get the etag file (same path as local but with added end ".etag")
    
        Directory.CreateDirectory(Path.GetDirectoryName(localFilePath)!); // Create the directory where the file will be 

        string remoteUrl = BaseWikiUrl + fileName; // build remote url
        using var request = new HttpRequestMessage(HttpMethod.Get, remoteUrl); // create request, where we will be asking for the file

        if (File.Exists(localFilePath) && File.Exists(etagFilePath)) // If we have the local file with the etag
        {
            string savedETag = await File.ReadAllTextAsync(etagFilePath); // Read content of the etag file
            request.Headers.IfNoneMatch.Add(new EntityTagHeaderValue($"\"{savedETag}\"")); // add etag to the request
        }

        try
        {
            var response = await HttpWikiClient.SendAsync(request); // sends the request and wait for response
        
            if (response.StatusCode == HttpStatusCode.NotModified) // if the remote file is not modified (the etag is the same)
            {
                // return content of the local file and IsOffline = 0  
                return (await File.ReadAllTextAsync(localFilePath), false); 
            }
            
            // if the etag is not the same, remote is newer, fetch the file from remote
            
            response.EnsureSuccessStatusCode(); // ensures the connection is good, if not raise the exception
            
            string newContent = await response.Content.ReadAsStringAsync(); // Get the new content of the file
            await File.WriteAllTextAsync(localFilePath, newContent); // write the new content of the file to cache

            if (response.Headers.ETag != null)
                await File.WriteAllTextAsync(etagFilePath, response.Headers.ETag.Tag.Trim('"')); // save the new etag to cache

            // successfully returns the new content from remote with IsOffline = 0  
            return (newContent, false);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GuidesService] Offline/error for {fileName}: {ex.Message}");
        
            // if we don't have internet connection or the connection broke, return the local cache file if exists
            if (File.Exists(localFilePath))
            {
                return (await File.ReadAllTextAsync(localFilePath), true);
            }
        
            // if we don't have the file cached, return null
            return (null, true);
        }
    }

    // Finds assets links in Markdown and downloads them as binary data
    private async Task CacheAssetsFromMarkdownAsync(string mdContent)
    {
        // Find links to assets using regex
        var matches = Regex.Matches(mdContent, @"\((_Assets/.*?)\)");
        
        foreach (Match match in matches)
        {
            if (match.Groups.Count > 1)
            {
                string assetPath = match.Groups[1].Value; // Take the asset base path
                string localFilePath = Path.Combine(_cacheDirectory, assetPath.Replace('/', Path.DirectorySeparatorChar)); // Make asset path to our cache
                string etagFilePath = localFilePath + ".etag"; // Make etag path
                string remoteUrl = BaseWikiUrl + assetPath; // create url to fetch the asset from wiki
    
                // Ensure the dir exists
                Directory.CreateDirectory(Path.GetDirectoryName(localFilePath)!);
    
                // Prepare request to ask for the etag
                using var request = new HttpRequestMessage(HttpMethod.Get, remoteUrl);
    
                // If etag exist insert it inside the request
                if (File.Exists(localFilePath) && File.Exists(etagFilePath))
                {
                    string savedETag = await File.ReadAllTextAsync(etagFilePath);
                    request.Headers.IfNoneMatch.Add(new EntityTagHeaderValue($"\"{savedETag}\""));
                }
    
                try
                {
                    var response = await HttpWikiClient.SendAsync(request);
    
                    if (response.StatusCode == HttpStatusCode.NotModified)
                    {
                        // Asset didnt change, skip the downloading
                        continue;
                    }
    
                    // Ensure the connection is ok
                    response.EnsureSuccessStatusCode();
    
                    // Download the asset and write it to cache
                    byte[] assetBytes = await response.Content.ReadAsByteArrayAsync();
                    await File.WriteAllBytesAsync(localFilePath, assetBytes);
    
                    // Save the new etag
                    if (response.Headers.ETag != null)
                    {
                        await File.WriteAllTextAsync(etagFilePath, response.Headers.ETag.Tag.Trim('"'));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[GuidesService] The image could not be downloaded or updated {assetPath}: {ex.Message}");
                }
            }
        }
    }
    
    // Method for showing images correctly in app (links for assets that work on wiki doesnt work in library and vice versa)
    public async Task<string?> GetWikiPageForDisplayAsync(string fileName)
    {
        // Get the content
        var result = await GetWikiFileTextAsync(fileName);
        string? mdContent = result.Content; 
    
        if (string.IsNullOrEmpty(mdContent))
            return null;

        // Get the absolute path
        string absoluteCacheUri = new Uri(_cacheDirectory).AbsoluteUri;
        mdContent = mdContent.Replace("(_Assets/", $"({absoluteCacheUri}/_Assets/"); // Replace old links with new links

        return mdContent; // return new content with correct links
    }
}