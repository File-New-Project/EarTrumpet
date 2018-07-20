using EarTrumpet.DataModel.Internal.Services;
using EarTrumpet.Extensions;
using EarTrumpet.Interop;
using EarTrumpet.Interop.MMDeviceAPI;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace EarTrumpet.DataModel.Internal
{
    class AudioDeviceManager : IMMNotificationClient, IAudioDeviceManager
    {
        public event EventHandler<IAudioDevice> DefaultChanged;
        public event EventHandler Loaded;

        public IAudioDeviceCollection Devices => _devices;

        private static AutoPolicyConfigClient s_PolicyConfigClient = null;

        private IMMDeviceEnumerator _enumerator;
        private IAudioDevice _defaultPlaybackDevice;
        private readonly Dispatcher _dispatcher;
        private readonly AudioDeviceCollection _devices;
        private readonly AudioPolicyConfigService _policyConfigService;

        public AudioDeviceManager(Dispatcher dispatcher)
        {
            Trace.WriteLine("AudioDeviceManager Create");

            _dispatcher = dispatcher;
            _devices = new AudioDeviceCollection();
            _policyConfigService = new AudioPolicyConfigService();

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

                    _dispatcher.BeginInvoke((Action)(() =>
                    {
                        QueryDefaultPlaybackDevice();
                        Loaded?.Invoke(this, null);
                    }));
                }
                catch (Exception ex)
                {
                    // Even through we're going to be broken, show the tray icon so the user can collect debug data.
                    AppTrace.LogWarning(ex);

                    _dispatcher.BeginInvoke((Action)(() =>
                    {
                        Loaded?.Invoke(this, null);
                    }));
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
            catch (Exception ex)
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
                _devices.TryFind(newDeviceId, out _defaultPlaybackDevice);

                DefaultChanged?.Invoke(this, _defaultPlaybackDevice);
            }
        }

        public IAudioDevice Default
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

        private void SetDefaultDevice(IAudioDevice device, ERole role = ERole.eMultimedia)
        {
            Trace.WriteLine($"AudioDeviceManager SetDefaultDevice {device.Id}");

            if (s_PolicyConfigClient == null)
            {
                s_PolicyConfigClient = new AutoPolicyConfigClient();
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

        public void MoveHiddenAppsToDevice(string appId, string id)
        {
            foreach (var device in _devices)
            {
                device.MoveHiddenAppsToDevice(appId, id);
            }
        }

        void IMMNotificationClient.OnDeviceAdded(string pwstrDeviceId)
        {
            Trace.WriteLine($"AudioDeviceManager OnDeviceAdded {pwstrDeviceId}");

            if (!_devices.TryFind(pwstrDeviceId, out IAudioDevice unused))
            {
                try
                {
                    IMMDevice device = _enumerator.GetDevice(pwstrDeviceId);
                    if (((IMMEndpoint)device).GetDataFlow() == EDataFlow.eRender)
                    {
                        var newDevice = new AudioDevice(this, device);

                        _dispatcher.BeginInvoke((Action)(() =>
                        {
                            // We must check again on the UI thread to avoid adding a duplicate device.
                            if (!_devices.TryFind(pwstrDeviceId, out IAudioDevice unused1))
                            {
                                _devices.Add(newDevice);
                            }
                        }));
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

            _dispatcher.BeginInvoke((Action)(() =>
            {
                if (_devices.TryFind(pwstrDeviceId, out IAudioDevice dev))
                {
                    _devices.Remove(dev);
                }
            }));
        }

        void IMMNotificationClient.OnDefaultDeviceChanged(EDataFlow flow, ERole role, string pwstrDefaultDeviceId)
        {
            Trace.WriteLine($"AudioDeviceManager OnDefaultDeviceChanged {pwstrDefaultDeviceId}");

            _dispatcher.BeginInvoke((Action)(() =>
            {
                QueryDefaultPlaybackDevice();
            }));
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
            Trace.WriteLine($"AudioDeviceManager OnPropertyValueChanged {pwstrDeviceId} {key.fmtid},{key.pid}");
            if (_devices.TryFind(pwstrDeviceId, out IAudioDevice dev))
            {
                if (PropertyKeys.PKEY_AudioEndPoint_Interface.Equals(key))
                {
                    // We're racing with the system, the device may not be resolvable anymore.
                    try
                    {
                        ((AudioDevice)dev).DevicePropertiesChanged(_enumerator.GetDevice(dev.Id));
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError($"{ex}");
                    }
                }
            }
        }

        public void SetDefaultEndPoint(string id, int pid)
        {
            _policyConfigService.SetDefaultEndPoint(id, pid);
        }

        public string GetDefaultEndPoint(int processId)
        {
            if (Environment.OSVersion.IsAtLeast(OSVersions.RS4))
            {
                return _policyConfigService.GetDefaultEndPoint(processId);
            }
            return null;
        }
    }
}
