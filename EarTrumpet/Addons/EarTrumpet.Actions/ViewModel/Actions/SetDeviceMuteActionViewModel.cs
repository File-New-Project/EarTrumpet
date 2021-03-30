using EarTrumpet.Actions.DataModel.Serialization;

namespace EarTrumpet.Actions.ViewModel.Actions
{
    class SetDeviceMuteActionViewModel : PartViewModel
    {
        public OptionViewModel Option { get; }
        public DeviceListViewModel Device { get; }

        private SetDeviceMuteAction _action;

        public SetDeviceMuteActionViewModel(SetDeviceMuteAction action) : base(action)
        {
            _action = action;
            Option = new OptionViewModel(action, nameof(action.Option));
            Device = new DeviceListViewModel(action, DeviceListViewModel.DeviceListKind.Recording | DeviceListViewModel.DeviceListKind.DefaultPlayback);

            Attach(Option);
            Attach(Device);
        }

        public override string LinkText
        {
            get
            {
                if (_action.Option == DataModel.Enum.MuteKind.ToggleMute)
                {
                    return Properties.Resources.SetDeviceMuteAction_LinkTextToggle;
                }
                else
                {
                    return base.LinkText;
                }
            }
        }
    }
}
