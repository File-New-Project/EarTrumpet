using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EarTrumpet.UI.ViewModels
{
    public class AddonAboutPageViewModel : SettingsPageViewModel
    {
        public AddonAboutPageViewModel(object addonObject) : base("Management")
        {
            Title = "About this add-on";
        }
    }
}
