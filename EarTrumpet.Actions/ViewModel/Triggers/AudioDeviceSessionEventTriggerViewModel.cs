using EarTrumpet_Actions.DataModel.Triggers;

namespace EarTrumpet_Actions.ViewModel.Triggers
{
    class AudioDeviceSessionEventTriggerViewModel : PartViewModel
    {
        public OptionViewModel Option { get; }

        public DeviceViewModel Device { get; }

        public AppViewModel App { get; }

        public AudioDeviceSessionEventTriggerViewModel(AudioDeviceSessionEventTrigger trigger) : base(trigger)
        {
            Option = new OptionViewModel(trigger);
            Option.PropertyChanged += (_, __) => UpdateDescription();
            Device = new DeviceViewModel(trigger);
            Device.PropertyChanged += (_, __) => UpdateDescription();
            App = new AppViewModel(trigger);
            App.PropertyChanged += (_, __) => UpdateDescription();
            UpdateDescription();
        }
    }
}
