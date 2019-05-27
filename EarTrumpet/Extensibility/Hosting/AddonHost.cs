using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace EarTrumpet.Extensibility.Hosting
{
    public class AddonHost
    {
        [ImportMany(typeof(IAddonLifecycle))]
        public List<IAddonLifecycle> AppLifecycleItems { get; set; }

        [ImportMany(typeof(IAddonContextMenu))]
        public List<IAddonContextMenu> TrayContextMenuItems { get; set; }

        [ImportMany(typeof(IAddonAppContent))]
        public List<IAddonAppContent> AppContentItems { get; set; }

        [ImportMany(typeof(IAddonDeviceContent))]
        public List<IAddonDeviceContent> DeviceContentItems { get; set; }

        [ImportMany(typeof(IAddonSettingsPage))]
        public List<IAddonSettingsPage> SettingsItems { get; set; }
    }
}
