using EarTrumpet.Extensibility.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EarTrumpet.UI.ViewModels
{
    public class AddonAboutPageViewModel : SettingsPageViewModel
    {
        Addon _addon;

        public string DisplayName => _addon.DisplayName;

        public AddonAboutPageViewModel(object addonObject) : base("Management")
        {
            Title = "About this add-on";
            _addon = AddonManager.Current.FindAddonForObject(addonObject);
        }
    }
}
