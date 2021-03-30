using EarTrumpet.Actions.DataModel.Serialization;

namespace EarTrumpet.Actions.ViewModel.Conditions
{
    class DefaultDeviceConditionViewModel : PartViewModel
    {
        public DeviceListViewModel Device { get; }

        public OptionViewModel Option { get; }

        public DefaultDeviceConditionViewModel(DefaultDeviceCondition condition) : base(condition)
        {
            Option = new OptionViewModel(condition, nameof(condition.Option));
            Device = new DeviceListViewModel(condition, DeviceListViewModel.DeviceListKind.Recording);

            Attach(Option);
            Attach(Device);
        }
    }
}
