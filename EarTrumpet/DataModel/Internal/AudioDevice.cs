using EarTrumpet.Extensions;
using EarTrumpet.Interop;
using EarTrumpet.Interop.MMDeviceAPI;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Threading;

namespace EarTrumpet.DataModel.Internal
{
    class AudioDevice : IAudioEndpointVolumeCallback, IAudioDevice, IAudioDeviceInternal
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private IMMDevice _device;
        private Dispatcher _dispatcher;
        private IAudioEndpointVolume _deviceVolume;
        private AudioDeviceSessionCollection _sessions;
        private IAudioMeterInformation _meter;
        private string _id;
        private string _displayName;
        private float _volume;
        private bool _isMuted;

        public AudioDevice(IMMDevice device)
        {
            _device = device;
            _dispatcher = App.Current.Dispatcher;
            _id = device.GetId();
            _deviceVolume = device.Activate<IAudioEndpointVolume>();

            _deviceVolume.RegisterControlChangeNotify(this);
            _meter = device.Activate<IAudioMeterInformation>();
            _sessions = new AudioDeviceSessionCollection(_device);

            _deviceVolume.GetMasterVolumeLevelScalar(out _volume);
            _isMuted = _deviceVolume.GetMute() != 0;

            ReadDisplayName();
        }

        ~AudioDevice()
        {
            _deviceVolume.UnregisterControlChangeNotify(this);
        }

        void IAudioEndpointVolumeCallback.OnNotify(ref AUDIO_VOLUME_NOTIFICATION_DATA pNotify)
        {
            _volume = pNotify.fMasterVolume;
            _isMuted = pNotify.bMuted != 0;

            _dispatcher.SafeInvoke(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Volume)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsMuted)));
            });
        }

        public float Volume
        {
            get => _volume;
            set
            {
                value = value.Bound(0, 1f);

                if (_volume != value)
                {
                    Guid dummy = Guid.Empty;
                    _deviceVolume.SetMasterVolumeLevelScalar(value, ref dummy);

                    IsMuted = false;
                }
            }
        }

        public float PeakValue => _meter.GetPeakValue();

        public bool IsMuted
        {
            get => _isMuted;
            set
            {
                if (value != _isMuted)
                {
                    Guid dummy = Guid.Empty;
                    _deviceVolume.SetMute(value ? 1 : 0, ref dummy);
                }
            }
        }

        public string Id => _id;

        public ObservableCollection<IAudioDeviceSession> Groups => _sessions.Sessions;

        public string DisplayName => _displayName;

        private void ReadDisplayName()
        {
            var propStore = _device.OpenPropertyStore(STGM.STGM_READ);
            var pv = propStore.GetValue(ref PropertyKeys.PKEY_Device_FriendlyName);
            _displayName = Marshal.PtrToStringUni(pv.union.pwszVal);
            Ole32.PropVariantClear(ref pv);
        }

        public void DevicePropertiesChanged(IMMDevice dev)
        {
            _device = dev;
            ReadDisplayName();

            _dispatcher.SafeInvoke(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayName)));
            });
        }
    }
}
