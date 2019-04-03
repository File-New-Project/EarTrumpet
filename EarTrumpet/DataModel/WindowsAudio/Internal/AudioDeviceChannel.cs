using System;
using System.ComponentModel;
using EarTrumpet.Interop.MMDeviceAPI;

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
            _level = _deviceVolume.GetChannelVolumeLevelScalar(index);
        }

        public float Level
        {
            get => _level;
            set
            {
                if (_level != value)
                {
                    Guid dummy = Guid.Empty;
                    _deviceVolume.SetChannelVolumeLevelScalar(_index, value, ref dummy);

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
