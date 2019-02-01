using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EarTrumpet.UI.ViewModels
{
    class EarTrumpetAboutPageViewModel : SettingsPageViewModel
    {

        public EarTrumpetAboutPageViewModel(string groupName) : base(groupName)
        {
            Title = "About";
        }
    }
}
