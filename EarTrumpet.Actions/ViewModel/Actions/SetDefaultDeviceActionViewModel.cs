using EarTrumpet_Actions.DataModel.Actions;

namespace EarTrumpet_Actions.ViewModel.Actions
{
    class SetDefaultDeviceActionViewModel : PartViewModel
    {
        public DeviceViewModel Device { get; }

        public SetDefaultDeviceActionViewModel(SetDefaultDeviceAction action) : base(action)
        {
            Device = new DeviceViewModel(action, DataModel.Device.DeviceListKind.Recording);
            Attach(Device);
        }
    }
}
