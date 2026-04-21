using System;
using System.Net.Http;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Material.Icons;

namespace PrintBuddy3D.ViewModels.Pages;

public partial class WikiPageViewModel(string displayName, MaterialIconKind icon, int index = 0) : PageBase(displayName, icon, index)
{
    private static readonly HttpClient Http = new();
    private const string BaseUrl = "https://raw.githubusercontent.com/wiki/MartinNovan/PrintBuddy3D/";

    [ObservableProperty] private string _content = "";
    [ObservableProperty] private string _footerContent = "";
    [ObservableProperty] private bool _isLoading = true;

    public async Task LoadAsync(string pageName)
    {
        IsLoading = true;
        try
        {
            // Get the body content from github wiki
            Content = await Http.GetStringAsync($"{BaseUrl}{pageName}.md");
            // Get footer content from github wiki
            FooterContent = await Http.GetStringAsync($"{BaseUrl}_Footer.md");
        }
        catch (Exception ex)
        {
            Content = $"Could not load website: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}