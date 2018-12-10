using EarTrumpet_Actions.DataModel.Actions;
using EarTrumpet_Actions.DataModel.Enum;

namespace EarTrumpet_Actions.ViewModel.Actions
{
    class SetDeviceVolumeActionViewModel : PartViewModel
    {
        public OptionViewModel Option { get; }
        public DeviceViewModel Device { get; }
        public VolumeViewModel Volume { get; }

        private SetDeviceVolumeAction _action;

        public SetDeviceVolumeActionViewModel(SetDeviceVolumeAction action) : base(action)
        {
            _action = action;
            Option = new OptionViewModel(action, nameof(action.Option));
            Device = new DeviceViewModel(action, DataModel.Device.DeviceListKind.Recording | DataModel.Device.DeviceListKind.DefaultPlayback);
            Volume = new VolumeViewModel(action);

            Attach(Option);
            Attach(Device);
            Attach(Volume);
        }

        public override string LinkText
        {
            get
            {
                if (_action.Option == SetVolumeKind.Set)
                {
                    return base.LinkText;
                }
                else
                {
                    return Properties.Resources.SetDeviceVolumeAction_LinkTextIncrement;
                }
            }
        }
    }
}
