using EarTrumpet.Extensibility;
using EarTrumpet.UI.Services;

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
                e.Icon = TrayIconFactory.Create(IconKind.OriginalIcon);
            }
        }
    }
}
