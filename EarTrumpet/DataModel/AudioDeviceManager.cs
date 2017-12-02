using EarTrumpet.Extensions;
using MMDeviceAPI_Interop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Threading;

namespace EarTrumpet.DataModel
{
    public class AudioDeviceManager : IMMNotificationClient
    {
        public event EventHandler<IAudioDevice> DefaultDeviceChanged;

        MMDeviceEnumerator m_enumerator;
        IAudioDevice m_defaultDevice;
        ObservableCollection<IAudioDevice> m_devices = new ObservableCollection<IAudioDevice>();
        VirtualDefaultAudioDevice m_virtualDefaultDevice;
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
            try
            {
                IMMDevice device;
                m_enumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia, out device);
                if (device != null)
                {
                    var id = device.GetId();
                    ((IMMNotificationClient)this).OnDeviceAdded(id);

                    m_defaultDevice = FindDevice(id);
                }
            }
            catch (COMException)
            {
                // No default device.
            }

            m_virtualDefaultDevice = new VirtualDefaultAudioDevice(this);
        }

        public VirtualDefaultAudioDevice VirtualDefaultDevice => m_virtualDefaultDevice;

        public IAudioDevice DefaultDevice
        {
            get
            {
                return m_defaultDevice;
            }
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

                    m_devices.Add(new AudioDevice(device, m_dispatcher));
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
                    if (m_defaultDevice == device)
                    {
                        m_defaultDevice = null;
                    }
                }
            });
        }

        void IMMNotificationClient.OnDefaultDeviceChanged(EDataFlow flow, ERole role, string pwstrDefaultDeviceId)
        {
            if (flow == EDataFlow.eRender && role != ERole.eCommunications)
            {
                m_dispatcher.SafeInvoke(() =>
                {
                    // Ensure the device is known.
                    ((IMMNotificationClient)this).OnDeviceAdded(pwstrDefaultDeviceId);

                    m_defaultDevice = FindDevice(pwstrDefaultDeviceId);
                    DefaultDeviceChanged?.Invoke(this, m_defaultDevice);
                });
            }
        }

        void IMMNotificationClient.OnDeviceStateChanged(string pwstrDeviceId, uint dwNewState) { }
        void IMMNotificationClient.OnPropertyValueChanged(string pwstrDeviceId, _tagpropertykey key) { }
    }
}
