using EarTrumpet.DataModel.Com;
using EarTrumpet.Extensions;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Threading;

namespace EarTrumpet.DataModel
{
    public class AudioDeviceManager : IMMNotificationClient, IAudioDeviceManager, IAudioDeviceManagerInternal
    {
        public event EventHandler<IAudioDevice> DefaultPlaybackDeviceChanged;
        public event EventHandler<IAudioDeviceSession> SessionCreated;

        IMMDeviceEnumerator _enumerator;
        IAudioDevice _defaultPlaybackDevice;
        IAudioDevice _defaultCommunicationsDevice;
        ObservableCollection<IAudioDevice> _devices = new ObservableCollection<IAudioDevice>();
        IVirtualDefaultAudioDevice _virtualDefaultDevice;
        Dispatcher _dispatcher;

        public AudioDeviceManager(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            _enumerator = (IMMDeviceEnumerator)new MMDeviceEnumerator();
            _enumerator.RegisterEndpointNotificationCallback(this);

            IMMDeviceCollection devices;
            _enumerator.EnumAudioEndpoints(EDataFlow.eRender, (uint)ERole.eMultimedia, out devices);

            uint deviceCount;
            devices.GetCount(out deviceCount);

            for (uint i = 0; i < deviceCount; i++)
            {
                IMMDevice immDevice;
                devices.Item(i, out immDevice);
                ((IMMNotificationClient)this).OnDeviceAdded(immDevice.GetId());
            }

            // Trigger default logic to register for volume change
            QueryDefaultPlaybackDevice();
            QueryDefaultCommunicationsDevice();

            _virtualDefaultDevice = new VirtualDefaultAudioDevice(this);
        }

        void QueryDefaultPlaybackDevice()
        {
            IMMDevice device = null;
            try
            {
                _enumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia, out device);
            }
            catch (COMException ex) when ((uint)ex.HResult == 0x80070490)
            {
                // Element not found.
            }

            string newDeviceId = null;

            if (device != null)
            {
                newDeviceId = device.GetId();
            }

            var currentDeviceId = _defaultPlaybackDevice != null ? _defaultPlaybackDevice.Id : null;

            if (currentDeviceId != newDeviceId)
            {
                if (newDeviceId == null) _defaultPlaybackDevice = null;
                else _defaultPlaybackDevice = FindDevice(newDeviceId);

                DefaultPlaybackDeviceChanged?.Invoke(this, _defaultPlaybackDevice);
            }
        }

        void QueryDefaultCommunicationsDevice()
        {
            IMMDevice device = null;
            try
            {
                _enumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eCommunications, out device);
            }
            catch (COMException ex) when ((uint)ex.HResult == 0x80070490)
            {
                // Element not found.
            }

            string newDeviceId = null;

            if (device != null)
            {
                newDeviceId = device.GetId();
            }

            var currentDeviceId = _defaultCommunicationsDevice != null ? _defaultCommunicationsDevice.Id : null;

            if (currentDeviceId != newDeviceId)
            {
                if (newDeviceId == null) _defaultCommunicationsDevice = null;
                else _defaultCommunicationsDevice = FindDevice(newDeviceId);

                // No event necessary.
            }
        }

        public IVirtualDefaultAudioDevice VirtualDefaultDevice => _virtualDefaultDevice;

        public IAudioDevice DefaultPlaybackDevice
        {
            get => _defaultPlaybackDevice;
            set
            {
                if (_defaultPlaybackDevice == null ||
                    value.Id != _defaultPlaybackDevice.Id)
                {
                    DefaultEndPoint.SetDefaultDevice(value);
                }
            }
        }

        public IAudioDevice DefaultCommunicationDevice
        {
            get => _defaultCommunicationsDevice;
            set
            {
                if (_defaultCommunicationsDevice == null ||
                    value.Id != _defaultCommunicationsDevice.Id)
                {
                    DefaultEndPoint.SetDefaultDevice(value, ERole.eCommunications);
                }
            }
        }


        public ObservableCollection<IAudioDevice> Devices => _devices;

        bool HasDevice(string deviceId)
        {
            return _devices.Any(d => d.Id == deviceId);
        }

        IAudioDevice FindDevice(string deviceId)
        {
            return _devices.First(d => d.Id == deviceId);
        }

        void IMMNotificationClient.OnDeviceAdded(string pwstrDeviceId)
        {
            _dispatcher.SafeInvoke(() =>
            {
                if (!HasDevice(pwstrDeviceId))
                {
                    try
                    {
                        IMMDevice device;
                        _enumerator.GetDevice(pwstrDeviceId, out device);

                        _devices.Add(new SafeAudioDevice(new AudioDevice(device, this, _dispatcher)));
                    }
                    catch(COMException ex)
                    {
                        Debug.WriteLine(ex);
                    }
                }
            });
        }

        void IMMNotificationClient.OnDeviceRemoved(string pwstrDeviceId)
        {
            _dispatcher.SafeInvoke(() =>
            {
                if (HasDevice(pwstrDeviceId))
                {
                    var device = FindDevice(pwstrDeviceId);
                    _devices.Remove(device);
                }
            });
        }

        void IMMNotificationClient.OnDefaultDeviceChanged(EDataFlow flow, ERole role, string pwstrDefaultDeviceId)
        {
            _dispatcher.SafeInvoke(() =>
            {
                QueryDefaultPlaybackDevice();
                QueryDefaultCommunicationsDevice();
            });
        }

        void IMMNotificationClient.OnDeviceStateChanged(string pwstrDeviceId, DEVICE_STATE dwNewState)
        {
            switch (dwNewState)
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

        void IMMNotificationClient.OnPropertyValueChanged(string pwstrDeviceId, PROPERTYKEY key) { }

        public void OnSessionCreated(IAudioDeviceSession session)
        {
            SessionCreated?.Invoke(this, session);
        }
    }
}
