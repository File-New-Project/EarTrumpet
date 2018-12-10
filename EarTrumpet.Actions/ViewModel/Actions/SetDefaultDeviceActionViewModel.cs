using EarTrumpet_Actions.DataModel.Serialization;

namespace EarTrumpet_Actions.ViewModel.Actions
{
    class SetDefaultDeviceActionViewModel : PartViewModel
    {
        public DeviceViewModel Device { get; }

        public SetDefaultDeviceActionViewModel(SetDefaultDeviceAction action) : base(action)
        {
            Device = new DeviceViewModel(action, DeviceViewModel.DeviceListKind.Recording);
            Attach(Device);
        }
    }
}
