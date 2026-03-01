using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using Material.Icons;
using Material.Icons.Avalonia;
using PrintBuddy3D.Views.Pages;
using SukiUI.Controls;
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

    public ObservableCollection<SukiSideMenuItem> WikiPages { get; set; } = new();
    private readonly ISukiToastManager _sukiToastManager;
    
    [ObservableProperty] private string _searchText = string.Empty;

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
        WikiPages.Clear();

        foreach (var dto in MenuStructure)
        {
            WikiPages.Add(CreateMenuItem(dto));
        }
    }

    private SukiSideMenuItem CreateMenuItem(MenuItemDto dto)
    {
        var item = new SukiSideMenuItem
        {
            Header = dto.Title,
            Classes = { "Compact" }
        };

        // Set the icon from loaded json
        if (!string.IsNullOrEmpty(dto.Icon) && Enum.TryParse(dto.Icon, out MaterialIconKind iconKind))
        {
            item.Icon = new MaterialIcon { Kind = iconKind };
        }

        // create wikipage
        if (!string.IsNullOrEmpty(dto.Page))
        {
            var pageVm = new WikiPageViewModel();
            _ = pageVm.LoadAsync(dto.Page);
            item.PageContent = new WikiPage { DataContext = pageVm };
        }

        // Return if no chlidren
        if (dto.Children is not { Count: > 0 }) return item;
        
        // add children the same way (recursive)
        foreach (var childDto in dto.Children)
        {
            item.Items.Add(CreateMenuItem(childDto));
        }

        return item;
    }
}