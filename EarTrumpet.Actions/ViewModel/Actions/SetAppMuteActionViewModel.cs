using EarTrumpet_Actions.DataModel.Serialization;

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
            App = new AppViewModel(action, AppViewModel.AppKind.EveryApp | AppViewModel.AppKind.ForegroundApp);
            Device = new DeviceViewModel(action, DeviceViewModel.DeviceListKind.DefaultPlayback);

            Attach(Option);
            Attach(App);
            Attach(Device);
        }
    }
}
