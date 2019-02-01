using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EarTrumpet.Extensibility;
using EarTrumpet.Extensibility.Shared;
using EarTrumpet.UI.ViewModels;
using EarTrumpet_Actions.Settings;

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
