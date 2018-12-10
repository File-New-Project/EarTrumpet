using EarTrumpet_Actions.DataModel.Serialization;

namespace EarTrumpet_Actions.ViewModel.Triggers
{
    class AppEventTriggerViewModel : PartViewModel
    {
        public OptionViewModel Option { get; }

        public DeviceViewModel Device { get; }

        public AppViewModel App { get; }

        public AppEventTriggerViewModel(AppEventTrigger trigger) : base(trigger)
        {
            Option = new OptionViewModel(trigger, nameof(trigger.Option));
            Device = new DeviceViewModel(trigger, DeviceViewModel.DeviceListKind.DefaultPlayback);
            App = new AppViewModel(trigger, AppViewModel.AppKind.Default);

            Attach(Option);
            Attach(Device);
            Attach(App);
        }
    }
}
