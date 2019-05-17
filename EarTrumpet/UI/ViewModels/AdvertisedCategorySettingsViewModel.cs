using EarTrumpet.Interop.Helpers;

namespace EarTrumpet.UI.ViewModels
{
    public class AdvertisedCategorySettingsViewModel : SettingsCategoryViewModel
    {
        string _link;

        public AdvertisedCategorySettingsViewModel(string title, string glyph, string description, string id, string link) : 
            base(title, glyph, description, id, new System.Collections.ObjectModel.ObservableCollection<SettingsPageViewModel>())
        {
            _link = link;
            IsAd = true;
        }

        public void Activate() => ProcessHelper.StartNoThrow(_link);
    }
}
