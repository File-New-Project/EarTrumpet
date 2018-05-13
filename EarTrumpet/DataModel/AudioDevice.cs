using EarTrumpet.Extensions;
using EarTrumpet.DataModel.Com;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using System.Diagnostics;
using EarTrumpet.Services;

namespace EarTrumpet.DataModel
{
    public class AudioDevice : IAudioEndpointVolumeCallback, IAudioDevice
    {
        public event PropertyChangedEventHandler PropertyChanged;

        IMMDevice _device;
        Dispatcher _dispatcher;
        IAudioEndpointVolume _deviceVolume;
        AudioDeviceSessionCollection _sessions;
        IAudioMeterInformation _meter;
        IAudioDeviceManagerInternal _manager;
        string _id;
        string _displayName;
        float _volume;
        bool _isMuted;

        public AudioDevice(IMMDevice device, IAudioDeviceManagerInternal manager, Dispatcher dispatcher)
        {
            _device = device;
            _dispatcher = dispatcher;
            _manager = manager;
            _id = device.GetId();
            _deviceVolume = device.Activate<IAudioEndpointVolume>();

            _deviceVolume.RegisterControlChangeNotify(this);
            _meter = device.Activate<IAudioMeterInformation>();
            _sessions = new AudioDeviceSessionCollection(_device, this, dispatcher);
            _sessions.Sessions.CollectionChanged += Sessions_CollectionChanged;

            ReadVolumeAndMute();

            ReadDisplayName();
        }

        ~AudioDevice()
        {
            _sessions.Sessions.CollectionChanged -= Sessions_CollectionChanged;
        }

        private void Sessions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    Debug.Assert(e.NewItems.Count == 1);
                    _manager.OnSessionCreated((IAudioDeviceSession)e.NewItems[0]);
                    break;
            }
        }

        void IAudioEndpointVolumeCallback.OnNotify(ref AUDIO_VOLUME_NOTIFICATION_DATA pNotify)
        {
            ReadVolumeAndMute();

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
                if (value > 1)
                {
                    value = 1.0f;
                }
                else if (value < 0)
                {
                    value = 0.0f;
                }

                if (_volume != value)
                {
                    Guid dummy = Guid.Empty;
                    _deviceVolume.SetMasterVolumeLevelScalar(value, ref dummy);
                }
            }
        }

        public float PeakValue
        {
            get
            {
                _meter.GetPeakValue(out float ret);
                return ret;
            }
        }

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

        public ObservableCollection<IAudioDeviceSession> Sessions => _sessions.Sessions;

        public string DisplayName => _displayName;

        private void ReadDisplayName()
        {
            IPropertyStore propStore;
            _device.OpenPropertyStore((uint)STGM.STGM_READ, out propStore);

            PROPERTYKEY PKEY_Device_FriendlyName = new PROPERTYKEY { fmtid = Guid.Parse("{0xa45c254e, 0xdf1c, 0x4efd, {0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0}}"), pid = new UIntPtr(14) };
            PropVariant pv;
            propStore.GetValue(ref PKEY_Device_FriendlyName, out pv);

            _displayName = Marshal.PtrToStringUni(pv.union.pwszVal);
            PropertyStoreInterop.PropVariantClear(ref pv);
        }

        private void ReadVolumeAndMute()
        {
            _deviceVolume.GetMasterVolumeLevelScalar(out _volume);
            _deviceVolume.GetMute(out int muted);
            _isMuted = muted != 0;
        }

        public void TakeSessionFromOtherDevice(int processId)
        {
            AudioPolicyConfigService.SetDefaultEndPoint(Id, processId);
        }
    }
}
