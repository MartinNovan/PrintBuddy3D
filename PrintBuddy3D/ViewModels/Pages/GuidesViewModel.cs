using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Material.Icons;
using Material.Icons.Avalonia;
using PrintBuddy3D.Views.Pages;
using SukiUI.Controls;

namespace PrintBuddy3D.ViewModels.Pages;

// Help class of items in side menu
public class MenuItemDto
{
    public string? Title { get; set; }
    public string? Icon { get; set; } // Icon name (must be material icon)
    public string? Page { get; set; } // file name without .md
    public List<MenuItemDto>? Children { get; set; }
}

public class GuidesViewModel : ObservableObject
{
    private ObservableCollection<MenuItemDto> MenuStructure { get; } = new();

    public ObservableCollection<SukiSideMenuItem> WikiPages { get; set; } = new();

    public GuidesViewModel()
    {
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
            BuildMenu();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Could not load _Menu.json from github wiki.");
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