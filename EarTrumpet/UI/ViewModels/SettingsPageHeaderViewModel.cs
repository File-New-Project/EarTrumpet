using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EarTrumpet.UI.ViewModels
{
    public class SettingsPageHeaderViewModel : BindableBase
    {
        public string Title => _settingsPageViewModel.Title;

        private SettingsPageViewModel _settingsPageViewModel;

        public SettingsPageHeaderViewModel(SettingsPageViewModel settingsPageViewModel)
        {
            _settingsPageViewModel = settingsPageViewModel;
        }
    }
}
