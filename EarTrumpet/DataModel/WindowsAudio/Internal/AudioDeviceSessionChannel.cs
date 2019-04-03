using EarTrumpet.Interop.MMDeviceAPI;
using System;
using System.ComponentModel;
using System.Windows.Threading;

namespace EarTrumpet.DataModel.WindowsAudio.Internal
{
    class AudioDeviceSessionChannel : BindableBase, INotifyPropertyChanged, IAudioDeviceSessionChannel
    {
        public float Level
        {
            get => _level;
            set
            {
                if (_level != value)
                {
                    _level = value;
                    Guid dummy = Guid.Empty;
                    _session.SetChannelVolume(_index, value, ref dummy);

                    RaisePropertyChanged(nameof(Level));
                }
            }
        }

        private readonly IChannelAudioVolume _session;
        private readonly uint _index;
        private readonly Dispatcher _dispatcher;
        private float _level;

        public AudioDeviceSessionChannel(IChannelAudioVolume session, uint index, Dispatcher dispatcher)
        {
            _session = session;
            _index = index;
            _dispatcher = dispatcher;
            session.GetChannelVolume(index, out _level);
        }

        internal void SetLevel(float newLevel)
        {
            _level = newLevel;
            _dispatcher.BeginInvoke(((Action)(() =>
            {
                RaisePropertyChanged(nameof(Level));
            })));
        }
    }
}
