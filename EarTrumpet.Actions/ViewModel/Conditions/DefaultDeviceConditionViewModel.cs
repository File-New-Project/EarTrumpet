using EarTrumpet_Actions.DataModel.Conditions;

namespace EarTrumpet_Actions.ViewModel.Conditions
{
    class DefaultDeviceConditionViewModel : PartViewModel
    {
        public DeviceViewModel Device { get; }

        public OptionViewModel Option { get; }

        public DefaultDeviceConditionViewModel(DefaultDeviceCondition condition) : base(condition)
        {
            Option = new OptionViewModel(condition, nameof(condition.Option));
            Device = new DeviceViewModel(condition, DataModel.Device.DeviceListKind.Recording);

            Attach(Option);
            Attach(Device);
        }
    }
}
