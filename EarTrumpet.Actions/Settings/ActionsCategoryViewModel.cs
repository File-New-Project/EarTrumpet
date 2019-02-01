using EarTrumpet.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EarTrumpet_Actions.Settings
{
    class ActionsCategoryViewModel : SettingsCategoryViewModel
    {
        public ActionsCategoryViewModel()
        {
            Title = "Actions";
            Description = "Create hotkeys and macros to control your audio experience";
            Glyph = "\xE164";

            Pages = new System.Collections.ObjectModel.ObservableCollection<SettingsPageViewModel>();
        }
    }
}
