using EarTrumpet.Extensions;
using MMDeviceAPI_Interop;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Threading;

namespace EarTrumpet.DataModel
{
    public class AudioDeviceManager : IMMNotificationClient, IAudioDeviceManager
    {
        // From mmdeviceapi.idl.
        enum DEVICE_STATE
        {
            ACTIVE    = 0x00000001,
            DISABLED  = 0x00000002,
            NOTPRESENT= 0x00000004,
            UNPLUGGED = 0x00000008,
            MASK_ALL  = 0x0000000f
        }

        public event EventHandler<IAudioDevice> DefaultDeviceChanged;

        MMDeviceEnumerator m_enumerator;
        IAudioDevice m_defaultDevice;
        ObservableCollection<IAudioDevice> m_devices = new ObservableCollection<IAudioDevice>();
        IVirtualDefaultAudioDevice m_virtualDefaultDevice;
        Dispatcher m_dispatcher;

        public AudioDeviceManager(Dispatcher dispatcher)
        {
            m_dispatcher = dispatcher;
            m_enumerator = new MMDeviceEnumerator();
            m_enumerator.RegisterEndpointNotificationCallback(this);

            IMMDeviceCollection devices;
            m_enumerator.EnumAudioEndpoints(EDataFlow.eRender, (uint)ERole.eMultimedia, out devices);

            uint deviceCount;
            devices.GetCount(out deviceCount);

            for (uint i = 0; i < deviceCount; i++)
            {
                IMMDevice immDevice;
                devices.Item(i, out immDevice);
                ((IMMNotificationClient)this).OnDeviceAdded(immDevice.GetId());
            }

            // Trigger default logic to register for volume change
            SetDefaultDevice();

            m_virtualDefaultDevice = new VirtualDefaultAudioDevice(this);
        }

        void SetDefaultDevice()
        {
            try
            {
                IMMDevice device;
                m_enumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia, out device);
                if (device != null)
                {
                    var id = device.GetId();

                    if (m_defaultDevice == null || m_defaultDevice.Id != id)
                    {
                        m_defaultDevice = FindDevice(id);
                        DefaultDeviceChanged?.Invoke(this, m_defaultDevice);
                    }
                }
            }
            catch (COMException)
            {
                m_defaultDevice = null;
            }
        }

        public IVirtualDefaultAudioDevice VirtualDefaultDevice => m_virtualDefaultDevice;

        public IAudioDevice DefaultDevice
        {
            get => m_defaultDevice;
            set
            {
                if (m_defaultDevice == null ||
                    value.Id != m_defaultDevice.Id)
                {
                    DefaultEndPoint.SetDefaultDevice(value);
                }
            }
        }

        public ObservableCollection<IAudioDevice> Devices => m_devices;

        bool HasDevice(string deviceId)
        {
            return m_devices.Any(d => d.Id == deviceId);
        }

        IAudioDevice FindDevice(string deviceId)
        {
            return m_devices.First(d => d.Id == deviceId);
        }

        void IMMNotificationClient.OnDeviceAdded(string pwstrDeviceId)
        {
            m_dispatcher.SafeInvoke(() =>
            {
                if (!HasDevice(pwstrDeviceId))
                {
                    IMMDevice device;
                    m_enumerator.GetDevice(pwstrDeviceId, out device);

                    m_devices.Add(new SafeAudioDevice(new AudioDevice(device, m_dispatcher)));
                }
            });
        }

        void IMMNotificationClient.OnDeviceRemoved(string pwstrDeviceId)
        {
            m_dispatcher.SafeInvoke(() =>
            {
                if (HasDevice(pwstrDeviceId))
                {
                    var device = FindDevice(pwstrDeviceId);
                    m_devices.Remove(device);
                }
            });
        }

        void IMMNotificationClient.OnDefaultDeviceChanged(EDataFlow flow, ERole role, string pwstrDefaultDeviceId)
        {
            m_dispatcher.SafeInvoke(() =>
            {
                SetDefaultDevice();
            });
        }

        void IMMNotificationClient.OnDeviceStateChanged(string pwstrDeviceId, uint dwNewState)
        {
            switch ((DEVICE_STATE)dwNewState)
            {
                case DEVICE_STATE.ACTIVE:
                    ((IMMNotificationClient)this).OnDeviceAdded(pwstrDeviceId);
                    break;
                case DEVICE_STATE.DISABLED:
                case DEVICE_STATE.NOTPRESENT:
                case DEVICE_STATE.UNPLUGGED:
                    ((IMMNotificationClient)this).OnDeviceRemoved(pwstrDeviceId);
                    break;
                default:
                    Debug.WriteLine($"Unknown DEVICE_STATE: {dwNewState}");
                    break;
            }
        }

        void IMMNotificationClient.OnPropertyValueChanged(string pwstrDeviceId, _tagpropertykey key) { }
    }
}
