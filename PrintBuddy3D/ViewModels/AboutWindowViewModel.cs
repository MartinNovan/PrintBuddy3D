using System.Diagnostics;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PrintBuddy3D.ViewModels;

public partial class AboutWindowViewModel : ObservableObject
{
    public string Version { get; }
    public string AppDescription => "Cross-platform 3D printer monitoring and control app.\nSupports Klipper and Marlin firmware.";
    public string AuthorName => "Martin Novan";
    public string GitHubUrl => "https://github.com/MartinNovan/PrintBuddy3D";
    public string LicenseText => "Licensed under GNU General Public License v3.0";

    public AcknowledgementItem[] Acknowledgements { get; } =
    [
        new("SukiUI",           "UI theme and component library",          "https://github.com/kikipoulet/SukiUI"),
        new("Avalonia UI",      "Cross-platform .NET UI framework",        "https://avaloniaui.net"),
        new("Dock",             "Docking layout system",                   "https://github.com/wieslawsoltes/Dock"),
        new("SSH.NET",          "SSH connectivity",                        "https://github.com/sshnet/SSH.NET"),
        new("LiveCharts2",      "Charts and data visualization",           "https://livecharts.dev"),
        new("Material Icons",   "Icon set for Avalonia",                   "https://github.com/AvaloniaUtils/Material.Icons.Avalonia"),
        new("Markdown.Avalonia","Markdown rendering",                      "https://github.com/whistyun/Markdown.Avalonia"),
        new("CommunityToolkit", "MVVM toolkit",                            "https://github.com/CommunityToolkit/dotnet"),
    ];

    public AboutWindowViewModel()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        Version = version is not null ? $"v{version.Major}.{version.Minor}.{version.Build}" : "X.X.X";
    }

    [RelayCommand]
    private void OpenUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
        }
        catch
        {
            // ignored
        }
    }
}
// helper class for acknowledment
public record AcknowledgementItem(string Name, string Description, string Url);