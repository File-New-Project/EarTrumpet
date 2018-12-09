using EarTrumpet_Actions.DataModel.Actions;

namespace EarTrumpet_Actions.ViewModel.Actions
{
    class SetDeviceMuteActionViewModel : PartViewModel
    {
        public OptionViewModel Option { get; }
        public DeviceViewModel Device { get; }

        private SetDeviceMuteAction _action;

        public SetDeviceMuteActionViewModel(SetDeviceMuteAction action) : base(action)
        {
            _action = action;
            Option = new OptionViewModel(action);
            Option.PropertyChanged += (_, __) => UpdateDescription();
            Device = new DeviceViewModel(action, DataModel.Device.DeviceListKind.Recording | DataModel.Device.DeviceListKind.DefaultPlayback);
            Device.PropertyChanged += (_, __) => UpdateDescription();
        }
    }
}
