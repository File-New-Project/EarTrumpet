using System;
using System.Collections.ObjectModel;

#if DEBUG
namespace EarTrumpet.DataModel.Audio.Mocks
{
    class AudioDevice : BindableBase, IAudioDevice
    {
        public AudioDevice(string id, IAudioDeviceManager parent)
        {
            Parent = parent;
            Id = id;
        }

        public string DisplayName => Id;

        public string IconPath => null;

        public IAudioDeviceManager Parent { get; }

        public ObservableCollection<IAudioDeviceSession> Groups { get; } = new ObservableCollection<IAudioDeviceSession>();

        public string Id { get; }

        private bool _isMuted;
        public bool IsMuted
        {
            get => _isMuted;
            set
            {
                if (_isMuted != value)
                {
                    _isMuted = value;
                    RaisePropertyChanged(nameof(IsMuted));
                }
            }
        }
        private float _volume = 1;
        public float Volume
        {
            get => _volume;
            set
            {
                if (_volume != value)
                {
                    _volume = value;
                    RaisePropertyChanged(nameof(Volume));
                }
            }
        }

        public float PeakValue1 { get; set; }

        public float PeakValue2 { get; set; }

        public void AddFilter(Func<ObservableCollection<IAudioDeviceSession>, ObservableCollection<IAudioDeviceSession>> filter)
        {
            throw new NotImplementedException();
        }
    }
}
#endif