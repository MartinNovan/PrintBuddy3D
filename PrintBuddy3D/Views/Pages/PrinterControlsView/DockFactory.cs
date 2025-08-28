using System;
using System.Collections.Generic;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm.Controls;
using PrintBuddy3D.ViewModels.Pages;

namespace PrintBuddy3D.Views.Pages.PrinterControlsView
{
    public class DockFactory(PrinterControlViewModel context) : Factory
    {
        private readonly PrinterControlViewModel _context = context;
        private IRootDock? _rootDock;

        public override IRootDock CreateLayout()
        {
            var movementControlViewModel = new MovementControlViewModel()
                { Id = "Movement", Title = "Movement" };
            var temperatureControlViewModel = new TempeatureControlViewModel()
                { Id = "Tempeature", Title = "Tempeature" };

            var rightDock = new ProportionalDock
            {
                Proportion = 0.75,
                Orientation = Orientation.Vertical,
                ActiveDockable = null,
                VisibleDockables = CreateList<IDockable>(
                    new ToolDock
                    {
                        Alignment = Alignment.Right,
                        ActiveDockable = movementControlViewModel,
                        VisibleDockables = CreateList<IDockable>(
                            movementControlViewModel
                        )
                    }
                )
            };

            var leftDock = new ToolDock
            {
                Alignment = Alignment.Left,
                VisibleDockables = CreateList<IDockable>(
                    new ToolDock
                    {
                        Alignment = Alignment.Right,
                        ActiveDockable = temperatureControlViewModel,
                        VisibleDockables = CreateList<IDockable>(
                            temperatureControlViewModel
                        )
                    }
                )
            };

            var mainLayout = new ProportionalDock
            {
                Orientation = Orientation.Horizontal,
                VisibleDockables = CreateList<IDockable>(
                    leftDock,
                    rightDock
                )
            };

            var mainControlViewModel = new MainControlViewModel
            {
                Id = "Main",
                Title = "Main",
                ActiveDockable = mainLayout,
                VisibleDockables = CreateList<IDockable>(mainLayout)
            };

            var rootDock = CreateRootDock();
            rootDock.IsCollapsable = false;
            rootDock.ActiveDockable = mainControlViewModel;
            rootDock.DefaultDockable = mainControlViewModel;
            rootDock.VisibleDockables = CreateList<IDockable>(mainControlViewModel);

            _rootDock = rootDock;
            return rootDock;
        }

        public override void InitLayout(IDockable layout)
        {
            ContextLocator = new Dictionary<string, Func<object?>>
            {
                ["Home"] = () => _context
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
}