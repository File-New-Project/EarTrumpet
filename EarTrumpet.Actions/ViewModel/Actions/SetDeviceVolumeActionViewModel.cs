using EarTrumpet_Actions.DataModel.Actions;

namespace EarTrumpet_Actions.ViewModel.Actions
{
    class SetDeviceVolumeActionViewModel : PartViewModel
    {
        private bool _isShowingVolume;
        public bool IsShowingVolume
        {
            get => _isShowingVolume;
            set
            {
                if (_isShowingVolume != value)
                {
                    _isShowingVolume = value;
                    RaisePropertyChanged(nameof(IsShowingVolume));
                }
            }
        }

        public OptionViewModel Option { get; }
        public DeviceViewModel Device { get; }
        public VolumeViewModel Volume { get; }

        private SetDeviceVolumeAction _action;

        public SetDeviceVolumeActionViewModel(SetDeviceVolumeAction action) : base(action)
        {
            _action = action;
            Option = new OptionViewModel(action);
            Option.PropertyChanged += (_, __) => UpdateDescription();
            Option.PropertyChanged += Option_PropertyChanged;
            Device = new DeviceViewModel(action, DataModel.Device.DeviceListKind.Recording | DataModel.Device.DeviceListKind.DefaultPlayback);
            Device.PropertyChanged += (_, __) => UpdateDescription();
            Volume = new VolumeViewModel(action);
            Volume.PropertyChanged += (_, __) => UpdateDescription();
            UpdateDescription();

            Option_PropertyChanged(null, null);
        }

        private void Option_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            IsShowingVolume = _action.Option ==  StreamActionKind.SetVolume;
        }
    }
}
