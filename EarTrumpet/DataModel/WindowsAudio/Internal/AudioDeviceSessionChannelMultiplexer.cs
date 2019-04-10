using System.ComponentModel;

namespace EarTrumpet.DataModel.WindowsAudio.Internal
{
    class AudioDeviceSessionChannelMultiplexer : BindableBase, IAudioDeviceSessionChannel
    {
        public float Level
        {
            get => _channels[0].Level;
            set
            {
                foreach(var channel in _channels)
                {
                    channel.Level = value;
                }
            }
        }

        private IAudioDeviceSessionChannel[] _channels;

        public AudioDeviceSessionChannelMultiplexer(IAudioDeviceSessionChannel[] channels)
        {
            _channels = channels;

            foreach(var channel in _channels)
            {
                channel.PropertyChanged += Channel_PropertyChanged;
            }
        }

        private void Channel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(e.PropertyName);
        }
    }
}
