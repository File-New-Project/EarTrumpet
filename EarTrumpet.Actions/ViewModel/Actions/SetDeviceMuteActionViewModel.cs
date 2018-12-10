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
            Option = new OptionViewModel(action, nameof(action.Option));
            Device = new DeviceViewModel(action, DeviceViewModel.DeviceListKind.Recording | DeviceViewModel.DeviceListKind.DefaultPlayback);

            Attach(Option);
            Attach(Device);
        }
    }
}
