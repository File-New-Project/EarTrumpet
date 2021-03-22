using EarTrumpet.Actions.DataModel.Serialization;

namespace EarTrumpet.Actions.ViewModel.Triggers
{
    class DeviceEventTriggerViewModel : PartViewModel
    {
        public OptionViewModel Option { get; }

        public DeviceListViewModel Device { get; }

        public DeviceEventTriggerViewModel(DeviceEventTrigger trigger) : base(trigger)
        {
            Option = new OptionViewModel(trigger, nameof(trigger.Option));
            Device = new DeviceListViewModel(trigger, DeviceListViewModel.DeviceListKind.DefaultPlayback | DeviceListViewModel.DeviceListKind.Recording);

            Attach(Option);
            Attach(Device);
        }
    }
}
