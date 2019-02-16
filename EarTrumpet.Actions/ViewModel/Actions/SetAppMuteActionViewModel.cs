using EarTrumpet.Actions.DataModel.Serialization;

namespace EarTrumpet.Actions.ViewModel.Actions
{
    class SetAppMuteActionViewModel : PartViewModel
    {
        public OptionViewModel Option { get; }
        public DeviceListViewModel Device { get; }
        public AppListViewModel App { get; }

        private SetAppMuteAction _action;

        public SetAppMuteActionViewModel(SetAppMuteAction action) : base(action)
        {
            _action = action;

            Option = new OptionViewModel(action, nameof(action.Option));
            App = new AppListViewModel(action, AppListViewModel.AppKind.EveryApp | AppListViewModel.AppKind.ForegroundApp);
            Device = new DeviceListViewModel(action, DeviceListViewModel.DeviceListKind.DefaultPlayback);

            Attach(Option);
            Attach(App);
            Attach(Device);
        }
    }
}
