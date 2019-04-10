using EarTrumpet.UI.Services;

namespace EarTrumpet.UI.ViewModels
{
    //hamed
    public class EarTrumpetStartupSettingsPageViewModel : SettingsPageViewModel
    {
        public bool Startup
        {
            get => SettingsService.Startup;
            set => SettingsService.Startup = value;
        }

        public EarTrumpetStartupSettingsPageViewModel() : base(null)
        {
            Title = Properties.Resources.StartupSettingsPageText;
            Glyph = "\xE7E8";
        }
    }
}
