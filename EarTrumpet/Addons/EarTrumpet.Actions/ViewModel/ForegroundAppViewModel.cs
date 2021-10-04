using EarTrumpet.UI.ViewModels;
using EarTrumpet.Actions.DataModel.Serialization;

namespace EarTrumpet.Actions.ViewModel
{
    class ForegroundAppViewModel : SettingsAppItemViewModel
    {
        public ForegroundAppViewModel()
        {
            Id = AppRef.ForegroundAppId;
            DisplayName = Properties.Resources.ForegroundAppText;
        }
    }
}
