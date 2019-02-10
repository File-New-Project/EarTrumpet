

using EarTrumpet_Actions.DataModel.Serialization;

namespace EarTrumpet_Actions.ViewModel
{
    class EveryAppViewModel : AppViewModelBase
    {
        public EveryAppViewModel()
        {
            DisplayName = Properties.Resources.EveryAppText;
            Id = App.EveryAppId;
        }
    }
}
