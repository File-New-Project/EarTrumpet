using EarTrumpet.Extensions;
using EarTrumpet.DataModel.Com;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace EarTrumpet.DataModel
{
    public class AudioDevice : IAudioEndpointVolumeCallback, IAudioDevice
    {
        static class Interop
        {
            [DllImport("combase.dll")]
            public static extern void RoGetActivationFactory(
                [MarshalAs(UnmanagedType.HString)] string activatableClassId,
                [In] ref Guid iid,
                [Out, MarshalAs(UnmanagedType.IInspectable)] out Object factory);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        IMMDevice _device;
        Dispatcher _dispatcher;
        IAudioEndpointVolume _deviceVolume;
        AudioDeviceSessionCollection _sessions;
        IAudioMeterInformation _meter;
        IAudioDeviceManagerInternal _manager;
        string _id;

        public AudioDevice(IMMDevice device, IAudioDeviceManagerInternal manager, Dispatcher dispatcher)
        {
            _device = device;
            _dispatcher = dispatcher;
            _manager = manager;
            _id = device.GetId();
            _deviceVolume = device.Activate<IAudioEndpointVolume>();

            _deviceVolume.RegisterControlChangeNotify(this);
            _meter = device.Activate<IAudioMeterInformation>();
            _sessions = new AudioDeviceSessionCollection(_device, dispatcher);
            _sessions.Sessions.CollectionChanged += Sessions_CollectionChanged;
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
            _dispatcher.SafeInvoke(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Volume)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsMuted)));
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
                ((IPropertyStore)propStore).GetValue(ref PKEY_Device_FriendlyName, out pv);

                var ret = Marshal.PtrToStringUni(pv.pointerValue);
                PropertyStoreInterop.PropVariantClear(ref pv);
                return ret;
            }
        }

        static IAudioPolicyConfigFactory s_sharedPolicyConfig;

        public void TakeSessionFromOtherDevice(IAudioDeviceSession session)
        {
            if (s_sharedPolicyConfig == null)
            {
                object factory;
                Guid iid = typeof(IAudioPolicyConfigFactory).GUID;
                Interop.RoGetActivationFactory("Windows.Media.Internal.AudioPolicyConfig", ref iid, out factory);
                s_sharedPolicyConfig = (IAudioPolicyConfigFactory)factory;
            }

            const string DEVINTERFACE_AUDIO_RENDER = "{e6327cad-dcec-4949-ae8a-991e976a79d2}";
            const string MMDEVAPI_TOKEN = @"\\?\SWD#MMDEVAPI";
            
            var persistedDeviceId = $"{MMDEVAPI_TOKEN}#{Id}#{DEVINTERFACE_AUDIO_RENDER}";
            s_sharedPolicyConfig.SetPersistedDefaultAudioEndpoint((uint)session.ProcessId, EDataFlow.eRender, ERole.eMultimedia & ERole.eConsole, persistedDeviceId);
        }

        public bool HasMeaningfulSessions()
        {
            foreach(var session in Sessions)
            {
                if (!session.IsSystemSoundsSession) return true;
            }
            return false;
        }
    }
}
