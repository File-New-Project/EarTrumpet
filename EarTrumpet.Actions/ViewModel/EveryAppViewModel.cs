using EarTrumpet.UI.ViewModels;
using EarTrumpet_Actions.DataModel.Serialization;

namespace EarTrumpet_Actions.ViewModel
{
    class EveryAppViewModel : SettingsAppItemViewModel
    {
        public EveryAppViewModel()
        {
            DisplayName = Properties.Resources.EveryAppText;
            Id = App.EveryAppId;
        }
    }
}
