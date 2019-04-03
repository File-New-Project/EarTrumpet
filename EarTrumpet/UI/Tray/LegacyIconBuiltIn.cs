using EarTrumpet.DataModel;
using EarTrumpet.Extensibility;
using EarTrumpet.UI.Services;
using System;
using System.Drawing;
using System.Windows;

namespace EarTrumpet.UI.Tray
{
    class LegacyIconBuiltIn : IAddonTrayIcon
    {
        public int Priority => 30;

        public LegacyIconBuiltIn()
        {
            SettingsService.UseLegacyIconChanged += SettingsService_UseLegacyIconChanged;
        }

        private void SettingsService_UseLegacyIconChanged(object sender, bool e)
        {
            ((App)App.Current).TrayViewModel.Refresh();
        }

        public void TrayIconChanging(AddonTrayIconEventArgs e)
        {
            if (SettingsService.UseLegacyIcon)
            {
                if (SystemSettings.IsSystemLightTheme)
                {
                    e.Icon = new Icon(Application.GetResourceStream(new Uri("pack://application:,,,/EarTrumpet;component/Assets/Application.ico")).Stream);
                }
                else
                {
                    e.Icon = TrayIconFactory.Create(IconKind.OriginalIcon);
                }
            }
        }
    }
}
