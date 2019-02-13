using EarTrumpet.DataModel;
using EarTrumpet.Extensibility;
using EarTrumpet.Interop.Helpers;
using System.Windows;

namespace EarTrumpet.UI.Tray
{
    class ThemeColorBuiltIn : IAddonTrayIcon
    {
        public int Priority => 20;

        public void TrayIconChanging(AddonTrayIconEventArgs e)
        {
            if (!SystemParameters.HighContrast)
            {
             //   if (SystemSettings.IsSystemLightTheme)
                {
                    e.Icon = IconUtils.ColorIcon(e.Icon, null);
                }
              //  else
                {
                    // It's the default so don't bother doing anything.
                }
            }
        }
    }
}
