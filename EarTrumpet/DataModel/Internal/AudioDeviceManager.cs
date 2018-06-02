using EarTrumpet.Extensions;
using EarTrumpet.Interop;
using EarTrumpet.Interop.MMDeviceAPI;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace EarTrumpet.DataModel.Internal
{
    class AudioDeviceManager : IMMNotificationClient, IAudioDeviceManager
    {
        public event EventHandler<IAudioDevice> DefaultPlaybackDeviceChanged;

        public ObservableCollection<IAudioDevice> Devices => _devices;

        private static IPolicyConfig s_PolicyConfigClient = null;

        private IMMDeviceEnumerator _enumerator;
        private IAudioDevice _defaultPlaybackDevice;
        private IAudioDevice _defaultCommunicationsDevice;
        private ObservableCollection<IAudioDevice> _devices = new ObservableCollection<IAudioDevice>();
        private IVirtualDefaultAudioDevice _virtualDefaultDevice;
        private Dispatcher _dispatcher;

        public AudioDeviceManager(Dispatcher dispatcher)
        {
            Trace.WriteLine("AudioDeviceManager Create");

            _dispatcher = dispatcher;
            _virtualDefaultDevice = new VirtualDefaultAudioDevice(this);

            Task.Factory.StartNew(() =>
            {
                try
                {
                    _enumerator = (IMMDeviceEnumerator)new MMDeviceEnumerator();
                    _enumerator.RegisterEndpointNotificationCallback(this);

                    var devices = _enumerator.EnumAudioEndpoints(EDataFlow.eRender, DeviceState.ACTIVE);
                    uint deviceCount = devices.GetCount();
                    for (uint i = 0; i < deviceCount; i++)
                    {
                        ((IMMNotificationClient)this).OnDeviceAdded(devices.Item(i).GetId());
                    }

                    // Trigger default logic to register for volume change
                    _dispatcher.SafeInvoke(() =>
                    {
                        QueryDefaultPlaybackDevice();
                        QueryDefaultCommunicationsDevice();
                    });
                }
                catch (Exception ex) when (ex.Is(Error.AUDCLNT_E_DEVICE_INVALIDATED))
                {
                    // Expected in some cases.
                }
            });

            Trace.WriteLine("AudioDeviceManager Create Exit");
        }

        ~AudioDeviceManager()
        {
            try
            {
                _enumerator.UnregisterEndpointNotificationCallback(this);
            }
            catch(Exception ex)
            {
                Trace.TraceError($"{ex}");
            }
        }

        private void QueryDefaultPlaybackDevice()
        {
            Trace.WriteLine("AudioDeviceManager QueryDefaultPlaybackDevice");
            IMMDevice device = null;
            try
            {
                device = _enumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia);
            }
            catch (Exception ex) when (ex.Is(Error.ERROR_NOT_FOUND))
            {
                // Expected.
            }

            string newDeviceId = device?.GetId();
            var currentDeviceId = _defaultPlaybackDevice?.Id;
            if (currentDeviceId != newDeviceId)
            {
                FindDevice(newDeviceId, out _defaultPlaybackDevice);

                DefaultPlaybackDeviceChanged?.Invoke(this, _defaultPlaybackDevice);
            }
        }

        private void QueryDefaultCommunicationsDevice()
        {
            Trace.WriteLine("AudioDeviceManager QueryDefaultCommunicationsDevice");
            IMMDevice device = null;
            try
            {
                device = _enumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eCommunications);
            }
            catch (Exception ex) when (ex.Is(Error.ERROR_NOT_FOUND))
            {
                // Expected.
            }

            string newDeviceId = device?.GetId();
            var currentDeviceId = _defaultCommunicationsDevice?.Id;
            if (currentDeviceId != newDeviceId)
            {
                FindDevice(newDeviceId, out _defaultCommunicationsDevice);
            }
        }

        public IVirtualDefaultAudioDevice VirtualDefaultDevice => _virtualDefaultDevice;

        public IAudioDevice DefaultPlaybackDevice
        {
            get => _defaultPlaybackDevice;
            set
            {
                if (_defaultPlaybackDevice == null || value.Id != _defaultPlaybackDevice.Id)
                {
                    SetDefaultDevice(value);
                }
            }
        }

        public IAudioDevice DefaultCommunicationDevice
        {
            get => _defaultCommunicationsDevice;
            set
            {
                if (_defaultCommunicationsDevice == null || value.Id != _defaultCommunicationsDevice.Id)
                {
                    SetDefaultDevice(value, ERole.eCommunications);
                }
            }
        }

        private void SetDefaultDevice(IAudioDevice device, ERole role = ERole.eMultimedia)
        {
            Trace.WriteLine($"AudioDeviceManager SetDefaultDevice {device.Id}");

            if (s_PolicyConfigClient == null)
            {
                s_PolicyConfigClient = (IPolicyConfig)new PolicyConfigClient();
            }

            // Racing with the system, the device may not be valid anymore.
            try
            {
                s_PolicyConfigClient.SetDefaultEndpoint(device.Id, role);
            }
            catch (Exception ex)
            {
                Trace.TraceError($"{ex}");
            }
        }

        private bool FindDevice(string deviceId, out IAudioDevice found)
        {
            if (deviceId == null)
            {
                found = null;
                return false;
            }

            found = _devices.FirstOrDefault(d => d.Id == deviceId);
            return found != null;
        }

        void IMMNotificationClient.OnDeviceAdded(string pwstrDeviceId)
        {
            Trace.WriteLine($"AudioDeviceManager OnDeviceAdded {pwstrDeviceId}");

            if (!FindDevice(pwstrDeviceId, out IAudioDevice unused))
            {
                try
                {
                    IMMDevice device = _enumerator.GetDevice(pwstrDeviceId);
                    if (((IMMEndpoint)device).GetDataFlow() == EDataFlow.eRender)
                    {
                        var newDevice = new AudioDevice(device);

                        _dispatcher.SafeInvoke(() =>
                        {
                            _devices.Add(newDevice);
                        });
                    }
                }
                catch (Exception ex)
                {
                    // We catch Exception here because IMMDevice::Activate can return E_POINTER/NullReferenceException, as well as other expcetions listed here:
                    // https://docs.microsoft.com/en-us/dotnet/framework/interop/how-to-map-hresults-and-exceptions
                    Trace.TraceError($"{ex}");
                }
            }
        }

        void IMMNotificationClient.OnDeviceRemoved(string pwstrDeviceId)
        {
            Trace.WriteLine($"AudioDeviceManager OnDeviceRemoved {pwstrDeviceId}");

            _dispatcher.SafeInvoke(() =>
            {
                if (FindDevice(pwstrDeviceId, out IAudioDevice dev))
                {
                    _devices.Remove(dev);
                }
            });
        }

        void IMMNotificationClient.OnDefaultDeviceChanged(EDataFlow flow, ERole role, string pwstrDefaultDeviceId)
        {
            Trace.WriteLine($"AudioDeviceManager OnDefaultDeviceChanged {pwstrDefaultDeviceId}");

            _dispatcher.SafeInvoke(() =>
            {
                QueryDefaultPlaybackDevice();
                QueryDefaultCommunicationsDevice();
            });
        }

        void IMMNotificationClient.OnDeviceStateChanged(string pwstrDeviceId, DeviceState dwNewState)
        {
            Trace.WriteLine($"AudioDeviceManager OnDeviceStateChanged {pwstrDeviceId} {dwNewState}");
            switch (dwNewState)
            {
                case DeviceState.ACTIVE:
                    ((IMMNotificationClient)this).OnDeviceAdded(pwstrDeviceId);
                    break;
                case DeviceState.DISABLED:
                case DeviceState.NOTPRESENT:
                case DeviceState.UNPLUGGED:
                    ((IMMNotificationClient)this).OnDeviceRemoved(pwstrDeviceId);
                    break;
                default:
                    Trace.TraceError($"Unknown DEVICE_STATE: {dwNewState}");
                    break;
            }
        }

        void IMMNotificationClient.OnPropertyValueChanged(string pwstrDeviceId, PROPERTYKEY key)
        {
            Trace.WriteLine($"AudioDeviceManager OnPropertyValueChanged {pwstrDeviceId} {key.fmtid}{key.pid}");
            if (FindDevice(pwstrDeviceId, out IAudioDevice dev))
            {
                if (PropertyKeys.PKEY_AudioEndPoint_Interface.Equals(key))
                {
                    // We're racing with the system, the device may not be resolvable anymore.
                    try
                    {
                        ((IAudioDeviceInternal)dev).DevicePropertiesChanged(_enumerator.GetDevice(dev.Id));
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError($"{ex}");
                    }
                }
            }
        }
    }
}
