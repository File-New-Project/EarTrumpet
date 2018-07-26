using EarTrumpet.UI.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace EarTrumpet.UI.ViewModels
{
    public class FocusedAppItemViewModel
    {
        public event Action RequestClose;

        public IAppItemViewModel App { get; }
        public ObservableCollection<ToolbarItem> Toolbar { get; }
        public string DisplayName => App.DisplayName;
        public bool IsMovable => App.IsMovable;

        public FocusedAppItemViewModel(DeviceCollectionViewModel parent, IAppItemViewModel app)
        {
            App = app;

            Toolbar = new ObservableCollection<ToolbarItem>();
            Toolbar.Add(new ToolbarItem
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
                    DisplayName = EarTrumpet.Properties.Resources.DefaultDeviceText,
                    IsChecked = (string.IsNullOrWhiteSpace(persistedDeviceId)),
                    Command = new RelayCommand(() =>
                    {
                        parent.MoveAppToDevice(app, null);
                        RequestClose.Invoke();
                    }),
                });
                items.Insert(1, new ContextMenuSeparator());
                Toolbar.Insert(0, new ToolbarItem
                {
                    GlyphFontSize = 16,
                    DisplayName = Properties.Resources.MoveButtonAccessibleText,
                    Glyph = "\uE8AB",
                    Menu = new ObservableCollection<ContextMenuItem>(items)
                });
            }
        }
    }
}
