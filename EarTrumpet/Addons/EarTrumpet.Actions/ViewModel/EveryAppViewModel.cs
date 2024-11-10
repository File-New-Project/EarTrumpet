using EarTrumpet.UI.ViewModels;
using EarTrumpet.Actions.DataModel.Serialization;

namespace EarTrumpet.Actions.ViewModel;

internal class EveryAppViewModel : SettingsAppItemViewModel
{
    public EveryAppViewModel()
    {
        DisplayName = Properties.Resources.EveryAppText;
        Id = AppRef.EveryAppId;
    }
}
