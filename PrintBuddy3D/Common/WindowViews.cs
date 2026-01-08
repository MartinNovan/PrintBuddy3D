using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace PrintBuddy3D.Common;

public class WindowViews
{
    private readonly Dictionary<Type, Type> _vmToViewMap = [];

    public void AddView<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TView, 
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TViewModel>(ServiceCollection services)
    {
        var viewType = typeof(TView);
        var viewModelType = typeof(TViewModel);

        _vmToViewMap.Add(viewModelType, viewType);
        services.AddSingleton(viewModelType);
    }

    public bool TryCreateView(object? viewModel, [NotNullWhen(true)] out Control? view)
    {
        view = null;

        if (viewModel == null)
        {
            return false;
        }

        var viewModelType = viewModel.GetType();

        if (_vmToViewMap.TryGetValue(viewModelType, out var viewType))
        {
            view = Activator.CreateInstance(viewType) as Control;

            if (view != null)
            {
                view.DataContext = viewModel;
            }
        }

        return view != null;
    }
}