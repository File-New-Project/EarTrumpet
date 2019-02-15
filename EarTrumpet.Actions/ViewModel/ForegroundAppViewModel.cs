using EarTrumpet.UI.ViewModels;
using EarTrumpet_Actions.DataModel.Serialization;

namespace EarTrumpet_Actions.ViewModel
{
    class ForegroundAppViewModel : SettingsAppItemViewModel
    {
        public ForegroundAppViewModel()
        {
            Id = App.ForegroundAppId;
            DisplayName = Properties.Resources.ForegroundAppText;
        }
    }
}
