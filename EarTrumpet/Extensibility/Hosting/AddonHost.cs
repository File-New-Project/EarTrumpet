using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace EarTrumpet.Extensibility.Hosting
{
    public class AddonHost
    {
        [ImportMany(typeof(EarTrumpetAddon))]
        public List<EarTrumpetAddon> Addons { get; set; }

        // Optional cast to any of the below:
        public List<IEarTrumpetAddonEvents> Events { get; set; }
        public List<IEarTrumpetAddonNotificationAreaContextMenu> TrayContextMenuItems { get; set; }
        public List<IEarTrumpetAddonAppContent> AppContentItems { get; set; }
        public List<IEarTrumpetAddonDeviceContent> DeviceContentItems { get; set; }
        public List<IEarTrumpetAddonSettingsPage> SettingsItems { get; set; }
    }
}
