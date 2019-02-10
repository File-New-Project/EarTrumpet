using EarTrumpet_Actions.DataModel.Serialization;

namespace EarTrumpet_Actions.ViewModel
{
    class ForegroundAppViewModel : AppViewModelBase
    {
        public ForegroundAppViewModel()
        {
            Id = App.ForegroundAppId;
            DisplayName = Properties.Resources.ForegroundAppText;
        }
    }
}
