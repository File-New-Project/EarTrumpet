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
        public SettingsCategoryViewModel Get(AddonInfo info)
        {
            ResourceLoader.Load(Addon.Namespace);
            return new ActionsCategoryViewModel(info);
        }
    }
}
