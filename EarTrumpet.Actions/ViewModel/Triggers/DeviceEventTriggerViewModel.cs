using EarTrumpet_Actions.DataModel.Triggers;

namespace EarTrumpet_Actions.ViewModel.Triggers
{
    class DeviceEventTriggerViewModel : PartViewModel
    {
        public OptionViewModel Option { get; }

        public DeviceViewModel Device { get; }

        public DeviceEventTriggerViewModel(DeviceEventTrigger trigger) : base(trigger)
        {
            Option = new OptionViewModel(trigger, nameof(trigger.Option));
            Device = new DeviceViewModel(trigger, DeviceViewModel.DeviceListKind.DefaultPlayback | DeviceViewModel.DeviceListKind.Recording);

            Attach(Option);
            Attach(Device);
        }
    }
}
