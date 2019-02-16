using EarTrumpet.Actions.DataModel.Serialization;
using EarTrumpet.Actions.DataModel.Enum;

namespace EarTrumpet.Actions.ViewModel.Actions
{
    class SetAppVolumeActionViewModel : PartViewModel
    {
        public OptionViewModel Option { get; }
        public DeviceListViewModel Device { get; }
        public AppListViewModel App { get; }
        public VolumeViewModel Volume { get; }

        private SetAppVolumeAction _action;

        public SetAppVolumeActionViewModel(SetAppVolumeAction action) : base(action)
        {
            _action = action;

            Option = new OptionViewModel(action, nameof(action.Option));
            App = new AppListViewModel(action, AppListViewModel.AppKind.EveryApp | AppListViewModel.AppKind.ForegroundApp);
            Device = new DeviceListViewModel(action, DeviceListViewModel.DeviceListKind.DefaultPlayback);
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
                    return Properties.Resources.SetAppVolumeAction_LinkTextIncrement;
                }
            }
        }
    }
}
