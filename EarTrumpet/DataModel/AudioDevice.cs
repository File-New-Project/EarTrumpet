using EarTrumpet.DataModel.Interfaces;
using EarTrumpet.Extensions;
using MMDeviceAPI_Interop;
using SoundControlAPI_Interop;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Threading;

namespace EarTrumpet.DataModel
{
    public class AudioDevice : IAudioEndpointVolumeCallback, IAudioDevice
    {
        public event PropertyChangedEventHandler PropertyChanged;

        IMMDevice m_device;
        Dispatcher m_dispatcher;
        IAudioEndpointVolume m_deviceVolume;
        AudioDeviceSessionCollection m_sessions;
        IAudioMeterInformation m_meter;
        string m_id;

        public AudioDevice(IMMDevice device, Dispatcher dispatcher)
        {
            m_device = device;
            m_dispatcher = dispatcher;
            m_id = device.GetId();
            m_deviceVolume = device.Activate<IAudioEndpointVolume>();

            m_deviceVolume.RegisterControlChangeNotify(this);
            m_meter = device.Activate<IAudioMeterInformation>();
            m_sessions = new AudioDeviceSessionCollection(m_device, dispatcher);
        }

        void IAudioEndpointVolumeCallback.OnNotify(ref AUDIO_VOLUME_NOTIFICATION_DATA pNotify)
        {
            m_dispatcher.SafeInvoke(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Volume"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsMuted"));
            });
        }

        public float Volume
        {
            get
            {
                m_deviceVolume.GetMasterVolumeLevelScalar(out float level);
                return level;
            }
            set
            {
                Guid dummy = Guid.Empty;
                m_deviceVolume.SetMasterVolumeLevelScalar(value, ref dummy);
            }
        }

        public float PeakValue
        {
            get
            {
                m_meter.GetPeakValue(out float ret);
                return ret;
            }
        }

        public bool IsMuted
        {
            get
            {
                m_deviceVolume.GetMute(out int muted);
                return muted != 0;
            }
            set
            {
                Guid dummy = Guid.Empty;
                m_deviceVolume.SetMute(value ? 1 : 0, ref dummy);
            }
        }

        public string Id => m_id;

        public AudioDeviceSessionCollection Sessions => m_sessions;

        public string DisplayName
        {
            get
            {
                IPropertyStore propStore;
                m_device.OpenPropertyStore(0 /*STGM_READ*/, out propStore);

                var propStoreShim = (IPropertyStore_Shim)propStore;

                PROPERTYKEY PKEY_Device_FriendlyName = new PROPERTYKEY { fmtid = GuidExtensions.FromLongFormat(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), pid = new UIntPtr(14) };
                PropVariant pv;
                propStoreShim.GetValue(ref PKEY_Device_FriendlyName, out  pv);

                return Marshal.PtrToStringUni(pv.pointerValue);

                // TODO PropVariantClear
            }
        }
    }
}
