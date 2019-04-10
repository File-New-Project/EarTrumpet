﻿using EarTrumpet.Extensibility;
using EarTrumpet.UI.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace EarTrumpet.UI.ViewModels
{
    public class FocusedAppItemViewModel : IFocusedViewModel
    {
        public static IAddonAppContent[] AddonContentItems { get; set; }

        public event Action RequestClose;

        public IAppItemViewModel App { get; }
        public ObservableCollection<ToolbarItemViewModel> Toolbar { get; }
        public string DisplayName => App.DisplayName;
        public ObservableCollection<object> Addons { get; }

        public FocusedAppItemViewModel(DeviceCollectionViewModel parent, IAppItemViewModel app)
        {
            App = app;

            Toolbar = new ObservableCollection<ToolbarItemViewModel>();
            Toolbar.Add(new ToolbarItemViewModel
            {
                GlyphFontSize = 10,
                DisplayName = Properties.Resources.CloseButtonAccessibleText,
                Glyph = "\uE8BB",
                Command = new RelayCommand(() => RequestClose.Invoke())
            });

            if (app.IsMovable)
            {
                var persistedDeviceId = app.PersistedOutputDevice;

                var items = parent.AllDevices.Select(dev => new ContextMenuItem
                {
                    DisplayName = dev.DisplayName,
                    Command = new RelayCommand(() =>
                    {
                        parent.MoveAppToDevice(app, dev);
                        RequestClose.Invoke();
                    }),
                    IsChecked = (dev.Id == persistedDeviceId),
                }).ToList();

                items.Insert(0, new ContextMenuItem
                {
                    DisplayName = Properties.Resources.DefaultDeviceText,
                    IsChecked = (string.IsNullOrWhiteSpace(persistedDeviceId)),
                    Command = new RelayCommand(() =>
                    {
                        parent.MoveAppToDevice(app, null);
                        RequestClose.Invoke();
                    }),
                });
                items.Insert(1, new ContextMenuSeparator());
                Toolbar.Insert(0, new ToolbarItemViewModel
                {
                    GlyphFontSize = 16,
                    DisplayName = Properties.Resources.MoveButtonAccessibleText,
                    Glyph = "\uE8AB",
                    Menu = new ObservableCollection<ContextMenuItem>(items)
                });
            }

            if (AddonContentItems != null)
            {
                Addons = new ObservableCollection<object>(AddonContentItems.Select(a => a.GetContentForApp(App.Parent.Id, App.Id, () => RequestClose.Invoke())).ToArray());

                var menuItems = AddonContentItems.SelectMany(a => a.GetItemsForApp(app.Parent.Id, app.AppId));
                if (menuItems.Any())
                {
                    Toolbar.Insert(0, new ToolbarItemViewModel
                    {
                        GlyphFontSize = 16,
                        DisplayName = Properties.Resources.MoreCommandsAccessibleText,
                        Glyph = "\uE10C",
                        Menu = new ObservableCollection<ContextMenuItem>(menuItems)
                    });
                }
            }
        }

        public void Closing()
        {

        }
    }
}
