using EarTrumpet.Extensibility;
using EarTrumpet.UI.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace EarTrumpet.UI.ViewModels
{
    class FocusedDeviceViewModel : IFocusedViewModel
    {
        public static IAddonDeviceContent[] AddonItems { get; set; }

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

            if (AddonItems != null)
            {
                Addons = new ObservableCollection<object>(AddonItems.Select(a => a.GetContentForDevice(device.Id)).ToArray());
            }
        }
    }
}
