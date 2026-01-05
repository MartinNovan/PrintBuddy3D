using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PrintBuddy3D.Common;

public class ViewLocator : IDataTemplate
{
    private readonly Dictionary<object, Control> _controlCache = [];

    public Control Build(object? param)
    {
        if (param is null)
        {
            return CreateText("Data is null.");
        }

        if (_controlCache.TryGetValue(param, out var control))
        {
            return control;
        }

        if (!App.WindowViews.TryCreateView(param, out var view))
            return CreateText($"No View For {param.GetType().Name}.");
        _controlCache.Add(param, view);
        return view;

    }

    public bool Match(object? data) => data is ObservableObject;

    private static TextBlock CreateText(string text) => new() { Text = text };
}