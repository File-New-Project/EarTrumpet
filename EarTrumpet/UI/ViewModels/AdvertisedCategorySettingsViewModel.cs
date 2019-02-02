using EarTrumpet.UI.Helpers;

namespace EarTrumpet.UI.ViewModels
{
    class AdvertisedCategorySettingsViewModel : SettingsCategoryViewModel
    {
        string _link;

        public AdvertisedCategorySettingsViewModel(string title, string glyph, string description, string link)
        {
            Title = title;
            Glyph = glyph;
            Description = description;
            _link = link;
        }

        internal void Activate()
        {
            ProcessHelper.StartNoThrow(_link);
        }
    }
}
