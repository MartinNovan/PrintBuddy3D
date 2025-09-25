using System;
using System.Collections.Generic;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm.Controls;
using PrintBuddy3D.ViewModels.Pages;

namespace PrintBuddy3D.Views.Pages.PrinterControlsDockView;

public class DockFactory(PrinterControlViewModel context) : Factory
{
    private IRootDock? _rootDock;
    public override IRootDock CreateLayout()
    {
        var movementControlViewModel = new MovementControlViewModel(context.PrinterControlService)
        { 
            Id = "MovementControl",
            Title = "Movement Control",
            CanPin = true,
            CanFloat = false,
            CanClose = false,
        };

        var temperatureControlViewModel = new TemperatureControlViewModel(context.PrinterControlService, context.Printer)
        { 
            Id = "TemperatureControl",
            Title = "Temperature Control",
            CanPin = true,
            CanFloat = false,
            CanClose = false,
        };

        var consoleControlViewModel = new PrinterConsoleControlViewModel(context.PrinterControlService)
        {
            Id = "Console",
            Title = "Printer Console",
            CanPin = true,
            CanFloat = false,
            CanClose = false,
        };

        var documentDock = new ToolDock()
        {
            IsCollapsable = true,
            ActiveDockable = movementControlViewModel,
            VisibleDockables = CreateList<IDockable>(movementControlViewModel, temperatureControlViewModel, consoleControlViewModel),
            CanClose = false,
            Title = "Document Control",
            Alignment = Alignment.Top,
        };

        var mainLayout = new ProportionalDock()
        {
            IsCollapsable = true,
            Orientation = Orientation.Horizontal,
            VisibleDockables = CreateList<IDockable>(documentDock),
        };
        
        var rootDock = CreateRootDock();
        rootDock.IsCollapsable = true;
        rootDock.CanFloat = false;
        rootDock.CanPin = true;
        rootDock.CanClose = false;
        rootDock.ActiveDockable = mainLayout;
        rootDock.DefaultDockable = mainLayout;
        rootDock.VisibleDockables = CreateList<IDockable>(mainLayout);

        _rootDock = rootDock;
        return rootDock;
    }

    public override void InitLayout(IDockable layout)
    {
        ContextLocator = new Dictionary<string, Func<object?>>
        {
            ["Home"] = () => context
        };

        DockableLocator = new Dictionary<string, Func<IDockable?>>()
        {
            ["Home"] = () => _rootDock,
        };


        HostWindowLocator = new Dictionary<string, Func<IHostWindow?>>
        {
            [nameof(IDockWindow)] = () => new HostWindow()
        };

        base.InitLayout(layout);
    }
}