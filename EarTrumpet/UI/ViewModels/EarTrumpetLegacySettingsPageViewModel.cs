using EarTrumpet.UI.Services;

namespace EarTrumpet.UI.ViewModels
{
    public class EarTrumpetLegacySettingsPageViewModel : SettingsPageViewModel
    {

        public bool UseLegacyIcon
        {
            get => SettingsService.UseLegacyIcon;
            set => SettingsService.UseLegacyIcon = value;
        }

        public EarTrumpetLegacySettingsPageViewModel() : base(null)
        {
            Title = Properties.Resources.LegacySettingsPageText;
            Glyph = "\xE825";
        }
    }
}