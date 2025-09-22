using System;
using System.Collections.Generic;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm.Controls;
using PrintBuddy3D.ViewModels.Pages;

namespace PrintBuddy3D.Views.Pages.PrinterControlsDockView
{
    public class DockFactory(PrinterControlViewModel context) : Factory
    {
        private IRootDock? _rootDock;
        public override IRootDock CreateLayout()
        {
            var movementControlViewModel = new MovementControlViewModel
            { 
                Id = "MovementControl",
                Title = "Movement Control",
                CanFloat = false, 
            };

            var temperatureControlViewModel = new TemperatureControlViewModel
            { 
                Id = "TempeatureControl",
                Title = "Tempeature Control",
                CanFloat = false, 
            };

            // Vytvoříme DocumentDock pro taby
            var documentDock = new DocumentDock()
            {
                IsCollapsable = false,
                ActiveDockable = movementControlViewModel,
                VisibleDockables = CreateList<IDockable>(movementControlViewModel, temperatureControlViewModel),
                CanCreateDocument = true,
                Title = "Document Control",
            };

            var mainLayout = new ProportionalDock()
            {
                Orientation = Orientation.Horizontal,
                VisibleDockables = CreateList<IDockable>(documentDock),
            };
            
            var mainControlViewModel = new MainControlViewModel
            {
                Id = "Home",
                Title = "Home",
                ActiveDockable = mainLayout,
                VisibleDockables = CreateList<IDockable>(mainLayout),
                CanFloat = false, 
            };

            var rootDock = CreateRootDock();
            rootDock.IsCollapsable = false;
            rootDock.CanFloat = false;
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
}