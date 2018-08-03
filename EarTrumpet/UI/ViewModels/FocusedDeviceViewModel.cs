using EarTrumpet.Extensibility;
using EarTrumpet.UI.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace EarTrumpet.UI.ViewModels
{
    class FocusedDeviceViewModel : IFocusedViewModel
    {
        public static IAddonDeviceContextMenu[] AddonContextMenuItems { get; set; }
        public static IAddonDeviceContent[] AddonContentItems { get; set; }

        public event Action RequestClose;

        public string DisplayName { get; }
        public ObservableCollection<ToolbarItemViewModel> Toolbar { get; }

        public ObservableCollection<object> Addons { get; }

        public FocusedDeviceViewModel(DeviceCollectionViewModel mainViewModel, DeviceViewModel device)
        {
            DisplayName = device.DisplayName;
            Toolbar = new ObservableCollection<ToolbarItemViewModel>();
            Toolbar.Add(new ToolbarItemViewModel
            {
                GlyphFontSize = 10,
                DisplayName = Properties.Resources.CloseButtonAccessibleText,
                Glyph = "\uE8BB",
                Command = new RelayCommand(() => RequestClose.Invoke())
            });

            if (AddonContentItems != null)
            {
                Addons = new ObservableCollection<object>(AddonContentItems.Select(a => a.GetContentForDevice(device.Id)).ToArray());
            }

            if (Features.IsEnabled(Feature.Addons) &&
                    AddonContextMenuItems != null && AddonContextMenuItems.Any())
            {
                var menuItems = AddonContextMenuItems.SelectMany(a => a.GetItemsForDevice(device.Id));
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
}
