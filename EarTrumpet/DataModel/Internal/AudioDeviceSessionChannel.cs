using EarTrumpet.Extensions;
using EarTrumpet.Interop;
using EarTrumpet.Interop.MMDeviceAPI;
using System;
using System.ComponentModel;
using System.Windows.Threading;

namespace EarTrumpet.DataModel.Internal
{
    class AudioDeviceSessionChannel : INotifyPropertyChanged, IAudioDeviceSessionChannel
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public float Level
        {
            get => _level;
            set
            {
                if (_level != value)
                {
                    _level = value;
                    try
                    {
                        _session.SetChannelVolume(_index, value);
                    }
                    catch (Exception ex) when (ex.Is(HRESULT.AUDCLNT_E_DEVICE_INVALIDATED))
                    {
                        // Expected in some cases.
                    }
                    
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Level)));
                }
            }
        }

        private readonly IChannelAudioVolume _session;
        private readonly int _index;
        private readonly Dispatcher _dispatcher;
        private float _level;

        public AudioDeviceSessionChannel(IChannelAudioVolume session, int index, Dispatcher dispatcher)
        {
            _session = session;
            _index = index;
            _dispatcher = dispatcher;
        }

        internal void SetLevel(float newLevel)
        {
            _level = newLevel;
            _dispatcher.BeginInvoke(((Action)(() => {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Level)));
            })));
        }
    }
}
