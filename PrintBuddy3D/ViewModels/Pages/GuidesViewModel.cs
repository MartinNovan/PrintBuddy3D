using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using Material.Icons;
using SukiUI.Toasts;

namespace PrintBuddy3D.ViewModels.Pages;

// Help class of items in side menu
public class MenuItemDto
{
    public string? Title { get; set; }
    public string? Icon { get; set; } // Icon name (must be material icon)
    public string? Page { get; set; } // file name without .md
    public List<MenuItemDto>? Children { get; set; }
}

public partial class GuidesViewModel : PageBase
{
    private ObservableCollection<MenuItemDto> MenuStructure { get; } = new();

    private readonly ISukiToastManager _sukiToastManager;
    public event EventHandler<PageBase>? NavigationRequested; // will be used after recreating UI in guides view

    public GuidesViewModel(ISukiToastManager sukiToastManager) : base("Guides", MaterialIconKind.Book, 3)
    {
        _sukiToastManager = sukiToastManager;
        _ = LoadMenuStructureAsync();
    }

    private async Task LoadMenuStructureAsync()
    {
        try
        {
            using var client = new HttpClient();
            // Get the menu, where all pages and hiearchy is stored
            var jsonUrl = "https://raw.githubusercontent.com/wiki/MartinNovan/PrintBuddy3D/_Menu.json";
            var json = await client.GetStringAsync(jsonUrl);

            var items = JsonSerializer.Deserialize<List<MenuItemDto>>(json);

            MenuStructure.Clear();
            if (items != null)
            {
                foreach (var item in items) MenuStructure.Add(item);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Could not load _Menu.json from github wiki: {ex.Message}");
            await Dispatcher.UIThread.InvokeAsync(() =>
                _sukiToastManager.CreateToast()
                    .WithTitle("Error")
                    .WithContent("Could not load guides from github wiki. \nPlease check your internet connection.")
                    .OfType(NotificationType.Error)
                    .Dismiss().ByClicking()
                    .Dismiss().After(TimeSpan.FromSeconds(30))
                    .Queue()
            );
        }
        finally
        {
            BuildMenu();
        }
    }
    
    private void BuildMenu()
    {
        SubPages.Clear();
        foreach (var dto in MenuStructure)
        {
            SubPages.Add(CreateMenuItem(dto));
        }
    }

    private PageBase CreateMenuItem(MenuItemDto dto)
    {
        var iconKind = MaterialIconKind.FileDocumentOutline; 
        if (!string.IsNullOrEmpty(dto.Icon) && Enum.TryParse(dto.Icon, out MaterialIconKind parsedIcon))
        {
            iconKind = parsedIcon;
        }
        var pageVm = new WikiPageViewModel(dto.Title ?? "Unknown Name", iconKind);
        
        if (!string.IsNullOrEmpty(dto.Page))
        {
            _ = pageVm.LoadAsync(dto.Page);
        }

        if (dto.Children is not { Count: > 0 }) return pageVm;
        foreach (var childDto in dto.Children)
        {
            pageVm.SubPages.Add(CreateMenuItem(childDto));
        }
        return pageVm;
    }
}