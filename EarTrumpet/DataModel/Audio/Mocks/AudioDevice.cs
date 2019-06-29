using EarTrumpet.DataModel.WindowsAudio;
using EarTrumpet.DataModel.WindowsAudio.Internal;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

#if DEBUG
namespace EarTrumpet.DataModel.Audio.Mocks
{
    class AudioDevice : BindableBase, IAudioDevice, IAudioDeviceInternal, IAudioDeviceWindowsAudio
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
        public IEnumerable<IAudioDeviceChannel> Channels { get; } = new List<IAudioDeviceChannel>();
        public string EnumeratorName => "Mock Enumerator";
        public string InterfaceName => "Mock Interface";
        public string DeviceDescription => "Mock Description";

        public void AddFilter(Func<ObservableCollection<IAudioDeviceSession>, ObservableCollection<IAudioDeviceSession>> filter)
        {
        }

        public void UpdatePeakValue()
        {
        }

        public void MoveHiddenAppsToDevice(string appId, string id)
        {
        }

        public void UnhideSessionsForProcessId(int processId)
        {
        }
    }
}
#endif