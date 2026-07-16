using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Material.Icons;
using PrintBuddy3D.Services;

namespace PrintBuddy3D.ViewModels.Pages;

public partial class WikiPageViewModel(IGuidesService guidesService, string displayName, MaterialIconKind icon, string fileName, int index = 0) : PageBase(displayName, icon, index)
{
    [ObservableProperty] private string _content = "";
    [ObservableProperty] private string _footerContent = "";

    private static string _sharedFooterContent = ""; // Footer is the same for all wiki pages, make one shared to save loading the footer
    // Method for loading content of the guide page (if page was loaded before, this method skips the loading)
    public async Task LoadIfNeededAsync()
    {
        try
        {
            // Get the body content from GitHub wiki
            if (string.IsNullOrEmpty(Content)) Content = await guidesService.GetWikiPageForDisplayAsync(fileName + ".md") ?? "";
            // Get footer content from GitHub wiki
            if (string.IsNullOrEmpty(FooterContent)) 
            {
                if (string.IsNullOrEmpty(_sharedFooterContent))
                {
                    _sharedFooterContent = await guidesService.GetWikiPageForDisplayAsync("_Footer.md") ?? "";
                }
                FooterContent = _sharedFooterContent;
            }
        }
        catch (Exception ex)
        {
            Content = $"Could not load wiki page: {ex.Message}";
        }
    }
}