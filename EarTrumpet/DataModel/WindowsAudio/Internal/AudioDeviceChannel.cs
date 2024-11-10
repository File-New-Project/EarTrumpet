using System;
using System.ComponentModel;
using Windows.Win32.Media.Audio.Endpoints;

namespace EarTrumpet.DataModel.WindowsAudio.Internal
{
    class AudioDeviceChannel : BindableBase, INotifyPropertyChanged, IAudioDeviceChannel
    {
        private float _level;
        private uint _index;
        private IAudioEndpointVolume _deviceVolume;

        public AudioDeviceChannel(IAudioEndpointVolume deviceVolume, uint index)
        {
            _index = index;
            _deviceVolume = deviceVolume;
            _deviceVolume.GetChannelVolumeLevelScalar(index, out _level);
        }

        public float Level
        {
            get => _level;
            set
            {
                if (_level != value)
                {
                    var context = Guid.Empty;
                    unsafe { _deviceVolume.SetChannelVolumeLevelScalar(_index, value, &context); }

                    _level = value;
                    RaisePropertyChanged(nameof(Level));
                }
            }
        }

        internal void OnNotify(float newLevel)
        {
            if (newLevel != _level)
            {
                _level = newLevel;
                RaisePropertyChanged(nameof(Level));
            }
        }
    }
}
