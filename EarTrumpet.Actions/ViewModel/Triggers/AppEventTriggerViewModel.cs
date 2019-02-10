using EarTrumpet_Actions.DataModel.Serialization;

namespace EarTrumpet_Actions.ViewModel.Triggers
{
    class AppEventTriggerViewModel : PartViewModel
    {
        public OptionViewModel Option { get; }

        public DeviceListViewModel Device { get; }

        public AppListViewModel App { get; }

        public AppEventTriggerViewModel(AppEventTrigger trigger) : base(trigger)
        {
            Option = new OptionViewModel(trigger, nameof(trigger.Option));
            Device = new DeviceListViewModel(trigger, DeviceListViewModel.DeviceListKind.DefaultPlayback);
            App = new AppListViewModel(trigger, AppListViewModel.AppKind.Default);

            Attach(Option);
            Attach(Device);
            Attach(App);
        }
    }
}
