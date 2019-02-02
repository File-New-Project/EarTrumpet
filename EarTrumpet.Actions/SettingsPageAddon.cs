using EarTrumpet.Extensibility;
using EarTrumpet.Extensibility.Shared;
using EarTrumpet.UI.ViewModels;
using EarTrumpet_Actions.ViewModel;
using System.ComponentModel.Composition;

namespace EarTrumpet_Actions
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
}
