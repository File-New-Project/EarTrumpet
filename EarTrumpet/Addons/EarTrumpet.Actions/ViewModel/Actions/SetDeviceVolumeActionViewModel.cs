using EarTrumpet.Actions.DataModel.Enum;
using EarTrumpet.Actions.DataModel.Serialization;

namespace EarTrumpet.Actions.ViewModel.Actions
{
    class SetDeviceVolumeActionViewModel : PartViewModel
    {
        public OptionViewModel Option { get; }
        public DeviceListViewModel Device { get; }
        public VolumeViewModel Volume { get; }

        private SetDeviceVolumeAction _action;

        public SetDeviceVolumeActionViewModel(SetDeviceVolumeAction action) : base(action)
        {
            _action = action;
            Option = new OptionViewModel(action, nameof(action.Option));
            Device = new DeviceListViewModel(action, DeviceListViewModel.DeviceListKind.Recording | DeviceListViewModel.DeviceListKind.DefaultPlayback);
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
