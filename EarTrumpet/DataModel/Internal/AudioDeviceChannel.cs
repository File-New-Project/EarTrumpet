using System;
using System.ComponentModel;
using EarTrumpet.Interop.MMDeviceAPI;

namespace EarTrumpet.DataModel.Internal
{
    class AudioDeviceChannel : INotifyPropertyChanged, IAudioDeviceChannel
    {
        public event PropertyChangedEventHandler PropertyChanged;

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
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Level)));
                }
            }
        }

        internal void OnNotify(float newLevel)
        {
          //  var newLevel = _deviceVolume.GetChannelVolumeLevelScalar(_index);
            if (newLevel != _level)
            {
                _level = newLevel;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Level)));
            }
        }
    }
}
