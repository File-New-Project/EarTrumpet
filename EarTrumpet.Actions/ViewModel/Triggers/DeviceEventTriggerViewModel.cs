using EarTrumpet_Actions.DataModel.Triggers;

namespace EarTrumpet_Actions.ViewModel.Triggers
{
    class DeviceEventTriggerViewModel : PartViewModel
    {
        public OptionViewModel Option { get; }

        public DeviceViewModel Device { get; }

        public DeviceEventTriggerViewModel(DeviceEventTrigger trigger) : base(trigger)
        {
            Option = new OptionViewModel(trigger);
            Option.PropertyChanged += (_, __) => UpdateDescription();
            Device = new DeviceViewModel(trigger, DataModel.Device.DeviceListKind.DefaultPlayback | DataModel.Device.DeviceListKind.Recording);
            Device.PropertyChanged += (_, __) => UpdateDescription();
            UpdateDescription();
        }
    }
}
