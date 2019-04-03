using EarTrumpet.DataModel;
using EarTrumpet.Extensibility;
using EarTrumpet.Interop.Helpers;
using System.Windows;
using System.Windows.Media;

namespace EarTrumpet.UI.Tray
{
    class ThemeColorBuiltIn : IAddonTrayIcon
    {
        public int Priority => 20;

        public void TrayIconChanging(AddonTrayIconEventArgs e)
        {
            if (!SystemParameters.HighContrast)
            {
                if (SystemSettings.IsSystemLightTheme)
                {
                    e.Icon = IconUtils.ColorIcon(e.Icon, e.Kind, Colors.Black);
                }
                else
                {
                    // It's the default so don't bother doing anything.
                }
            }
        }
    }
}
