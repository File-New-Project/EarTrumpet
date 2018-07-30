using EarTrumpet_Actions.DataModel.Triggers;

namespace EarTrumpet_Actions.ViewModel.Triggers
{
    class AppEventTriggerViewModel : PartViewModel
    {
        public OptionViewModel Option { get; }

        public DeviceViewModel Device { get; }

        public AppViewModel App { get; }

        public AppEventTriggerViewModel(AppEventTrigger trigger) : base(trigger)
        {
            Option = new OptionViewModel(trigger);
            Option.PropertyChanged += (_, __) => UpdateDescription();
            Device = new DeviceViewModel(trigger, DataModel.Device.DeviceListKind.DefaultPlayback);
            Device.PropertyChanged += (_, __) => UpdateDescription();
            App = new AppViewModel(trigger);
            App.PropertyChanged += (_, __) => UpdateDescription();
            UpdateDescription();
        }
    }
}
