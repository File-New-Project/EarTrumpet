using EarTrumpet.Extensibility;
using EarTrumpet.Extensibility.Shared;
using EarTrumpet.UI.ViewModels;
using EarTrumpet.Actions.ViewModel;
using System.ComponentModel.Composition;

namespace EarTrumpet.Actions
{
    [Export(typeof(IAddonSettingsPage))]
    class SettingsPageAddon : IAddonSettingsPage
    {
        public SettingsCategoryViewModel Get()
        {
            ResourceLoader.Load(Addon.Namespace);
            return new ActionsCategoryViewModel();
        }
    }

#if DEBUG
    [Export(typeof(IAddonSettingsPage))]
    class SettingsPageAddonAd : IAddonSettingsPage
    {
        public SettingsCategoryViewModel Get()
        {
            return new AdvertisedCategorySettingsViewModel(
                Properties.Resources.MyActionsText, "\xE950", Properties.Resources.AddonDescriptionText, "EarTrumpet.project-eta-adonly", "https://github.com/File-New-Project/EarTrumpet");
        }
    }
#endif
}
