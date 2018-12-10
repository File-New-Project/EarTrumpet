using EarTrumpet_Actions.DataModel.Actions;
using EarTrumpet_Actions.DataModel.Enum;

namespace EarTrumpet_Actions.ViewModel.Actions
{
    class SetAppVolumeActionViewModel : PartViewModel
    {
        public OptionViewModel Option { get; }
        public DeviceViewModel Device { get; }
        public AppViewModel App { get; }
        public VolumeViewModel Volume { get; }

        private SetAppVolumeAction _action;

        public SetAppVolumeActionViewModel(SetAppVolumeAction action) : base(action)
        {
            _action = action;

            Option = new OptionViewModel(action, nameof(action.Option));
            App = new AppViewModel(action, DataModel.App.AppKind.EveryApp | DataModel.App.AppKind.ForegroundApp);
            Device = new DeviceViewModel(action, DataModel.Device.DeviceListKind.DefaultPlayback);
            Volume = new VolumeViewModel(action);

            Attach(Option);
            Attach(App);
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
                    return Properties.Resources.SetAppVolumeActionLinkTextIncrement;
                }
            }
        }
    }
}
