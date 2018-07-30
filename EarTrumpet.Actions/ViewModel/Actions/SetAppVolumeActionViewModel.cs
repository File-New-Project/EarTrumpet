using EarTrumpet_Actions.DataModel.Actions;

namespace EarTrumpet_Actions.ViewModel.Actions
{
    class SetAppVolumeActionViewModel : PartViewModel
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
        public AppViewModel App { get; }
        public VolumeViewModel Volume { get; }

        private SetAppVolumeAction _action;

        public SetAppVolumeActionViewModel(SetAppVolumeAction action) : base(action)
        {
            _action = action;

            Option = new OptionViewModel(action);
            Option.PropertyChanged += (_, __) => UpdateDescription();
            Option.PropertyChanged += Option_PropertyChanged;
            App = new AppViewModel(action);
            App.PropertyChanged += (_, __) => UpdateDescription();
            Device = new DeviceViewModel(action);
            Device.PropertyChanged += (_, __) => UpdateDescription();
            Volume = new VolumeViewModel(action);
            Volume.PropertyChanged += (_, __) => UpdateDescription();
            UpdateDescription();

            Option_PropertyChanged(null, null);
        }

        private void Option_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            IsShowingVolume = _action.Option == StreamActionKind.SetVolume;
        }
    }
}
