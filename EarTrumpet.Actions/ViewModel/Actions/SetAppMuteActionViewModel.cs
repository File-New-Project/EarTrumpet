using EarTrumpet_Actions.DataModel.Actions;

namespace EarTrumpet_Actions.ViewModel.Actions
{
    class SetAppMuteActionViewModel : PartViewModel
    {
        public OptionViewModel Option { get; }
        public DeviceViewModel Device { get; }
        public AppViewModel App { get; }

        private SetAppMuteAction _action;

        public SetAppMuteActionViewModel(SetAppMuteAction action) : base(action)
        {
            _action = action;

            Option = new OptionViewModel(action, nameof(action.Option));
            App = new AppViewModel(action, DataModel.App.AppKind.EveryApp | DataModel.App.AppKind.ForegroundApp);
            Device = new DeviceViewModel(action, DataModel.Device.DeviceListKind.DefaultPlayback);

            Attach(Option);
            Attach(App);
            Attach(Device);
        }
    }
}
