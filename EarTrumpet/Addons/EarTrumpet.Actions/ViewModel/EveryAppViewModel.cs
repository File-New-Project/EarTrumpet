using EarTrumpet.UI.ViewModels;
using EarTrumpet.Actions.DataModel.Serialization;

namespace EarTrumpet.Actions.ViewModel
{
    class EveryAppViewModel : SettingsAppItemViewModel
    {
        public EveryAppViewModel()
        {
            DisplayName = Properties.Resources.EveryAppText;
            Id = AppRef.EveryAppId;
        }
    }
}
