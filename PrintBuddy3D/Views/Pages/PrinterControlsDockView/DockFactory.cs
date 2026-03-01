using System;
using System.Collections.Generic;
using Avalonia.Threading;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm.Controls;
using PrintBuddy3D.ViewModels.Pages;

namespace PrintBuddy3D.Views.Pages.PrinterControlsDockView;

public class DockFactory(PrinterControlViewModel context) : Factory, IDisposable
{
    private IRootDock? _rootDock;
    private MovementControlViewModel? _movementViewModel;
    private TemperatureControlViewModel? _temperatureViewModel;
    private PrinterConsoleControlViewModel? _consoleViewModel;
    public override IRootDock CreateLayout()
    {
        _movementViewModel = new MovementControlViewModel(context.PrinterControlService)
        { 
            Id = "MovementControl",
            Title = "Movement Control",
            CanPin = true,
            CanFloat = false,
            CanClose = false,
        };

        _temperatureViewModel = new TemperatureControlViewModel(context.PrinterControlService, context.Printer)
        { 
            Id = "TemperatureControl",
            Title = "Temperature Control",
            CanPin = true,
            CanFloat = false,
            CanClose = false,
        };

        _consoleViewModel = new PrinterConsoleControlViewModel(context.PrinterControlService)
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
            ActiveDockable = _movementViewModel,
            VisibleDockables = CreateList<IDockable>(_movementViewModel, _temperatureViewModel, _consoleViewModel),
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

        var rootDock = Dispatcher.UIThread.Invoke(() => 
        {
            var dock = CreateRootDock();
            dock.IsCollapsable = true;
            dock.CanFloat = false;
            dock.CanPin = true;
            dock.CanClose = false;
            dock.ActiveDockable = mainLayout;
            dock.DefaultDockable = mainLayout;
            dock.VisibleDockables = CreateList<IDockable>(mainLayout);
    
            return dock;
        });
        _rootDock = rootDock;
        return rootDock;
    }

    public void Dispose()
    {
        _movementViewModel?.Dispose();
        _temperatureViewModel?.Dispose();
        _consoleViewModel?.Dispose();
        _movementViewModel = null;
        _temperatureViewModel = null;
        _consoleViewModel = null;
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