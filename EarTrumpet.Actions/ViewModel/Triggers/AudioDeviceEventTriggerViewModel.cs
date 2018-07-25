using EarTrumpet_Actions.DataModel.Triggers;

namespace EarTrumpet_Actions.ViewModel.Triggers
{
    class AudioDeviceEventTriggerViewModel : PartViewModel
    {
        public OptionViewModel Option { get; }

        public DeviceViewModel Device { get; }

        public AudioDeviceEventTriggerViewModel(AudioDeviceEventTrigger trigger) : base(trigger)
        {
            Option = new OptionViewModel(trigger);
            Option.PropertyChanged += (_, __) => UpdateDescription();
            Device = new DeviceViewModel(trigger);
            Device.PropertyChanged += (_, __) => UpdateDescription();
            UpdateDescription();
        }
    }
}
