using EarTrumpet.Extensions;
using EarTrumpet.DataModel.Com;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using System.Collections.ObjectModel;

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
        string _id;

        public AudioDevice(IMMDevice device, Dispatcher dispatcher)
        {
            _device = device;
            _dispatcher = dispatcher;
            _id = device.GetId();
            _deviceVolume = device.Activate<IAudioEndpointVolume>();

            _deviceVolume.RegisterControlChangeNotify(this);
            _meter = device.Activate<IAudioMeterInformation>();
            _sessions = new AudioDeviceSessionCollection(_device, dispatcher);
        }

        void IAudioEndpointVolumeCallback.OnNotify(ref AUDIO_VOLUME_NOTIFICATION_DATA pNotify)
        {
            _dispatcher.SafeInvoke(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Volume"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsMuted"));
            });
        }

        public float Volume
        {
            get
            {
                _deviceVolume.GetMasterVolumeLevelScalar(out float level);
                return level;
            }
            set
            {
                Guid dummy = Guid.Empty;
                _deviceVolume.SetMasterVolumeLevelScalar(value, ref dummy);
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
            get
            {
                _deviceVolume.GetMute(out int muted);
                return muted != 0;
            }
            set
            {
                Guid dummy = Guid.Empty;
                _deviceVolume.SetMute(value ? 1 : 0, ref dummy);
            }
        }

        public string Id => _id;

        public ObservableCollection<IAudioDeviceSession> Sessions => _sessions.Sessions;

        public string DisplayName
        {
            get
            {
                IPropertyStore propStore;
                _device.OpenPropertyStore((uint)STGM.STGM_READ, out propStore);

                PROPERTYKEY PKEY_Device_FriendlyName = new PROPERTYKEY { fmtid = Guid.Parse("{0xa45c254e, 0xdf1c, 0x4efd, {0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0}}"), pid = new UIntPtr(14) };
                PropVariant pv;
                ((IPropertyStore)propStore).GetValue(ref PKEY_Device_FriendlyName, out  pv);

                var ret = Marshal.PtrToStringUni(pv.pointerValue);
                PropertyStoreInterop.PropVariantClear(ref pv);
                return ret;
            }
        }
    }
}
