using CommunityToolkit.Mvvm.ComponentModel;
using Material.Icons;

namespace PrintBuddy3D.ViewModels.Pages;

public abstract class PageBase(string displayName, MaterialIconKind icon, int index) : ObservableObject
{
    public string DisplayName { get; } = displayName;
    public MaterialIconKind Icon { get; } = icon;
    public int Index { get; } = index;
}