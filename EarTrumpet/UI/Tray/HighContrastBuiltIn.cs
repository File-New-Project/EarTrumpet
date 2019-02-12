using EarTrumpet.Extensibility;
using EarTrumpet.Interop.Helpers;
using System.Windows;

namespace EarTrumpet.UI.Tray
{
    class HighContrastBuiltIn : IAddonTrayIcon
    {
        public int Priority => 10;

        public void TrayIconChanging(AddonTrayIconEventArgs e)
        {
            if (SystemParameters.HighContrast)
            {
                e.Icon = IconUtils.ColorIcon(e.Icon, SystemColors.MenuTextColor);
            }
        }
    }
}
