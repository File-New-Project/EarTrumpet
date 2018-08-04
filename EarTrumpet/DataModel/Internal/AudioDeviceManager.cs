using EarTrumpet.DataModel.Internal.Services;
using EarTrumpet.Extensions;
using EarTrumpet.Interop;
using EarTrumpet.Interop.MMDeviceAPI;
using System;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace EarTrumpet.DataModel.Internal
{
    class AudioDeviceManager : IMMNotificationClient, IAudioDeviceManager
    {
        public event EventHandler<IAudioDevice> DefaultChanged;
        public event EventHandler Loaded;

        public IAudioDeviceCollection Devices => _devices;
        public AudioDeviceKind DeviceKind => _kind;

        private EDataFlow Flow => _kind == AudioDeviceKind.Playback ? EDataFlow.eRender : EDataFlow.eCapture;

        private static AutoPolicyConfigClientWin7 s_PolicyConfigClient = null;

        private IMMDeviceEnumerator _enumerator;
        private IAudioDevice _default;
        private readonly Dispatcher _dispatcher;
        private readonly AudioDeviceKind _kind;
        private readonly AudioDeviceCollection _devices;
        private readonly AudioPolicyConfigService _policyConfigService;

        public AudioDeviceManager(AudioDeviceKind kind)
        {
            _kind = kind;
            _dispatcher = Dispatcher.CurrentDispatcher;
            _devices = new AudioDeviceCollection();
            _policyConfigService = new AudioPolicyConfigService(Flow);

            TraceLine($"Create");

            Task.Factory.StartNew(() =>
            {
                try
                {
                    _enumerator = (IMMDeviceEnumerator)new MMDeviceEnumerator();
                    _enumerator.RegisterEndpointNotificationCallback(this);

                    var devices = _enumerator.EnumAudioEndpoints(Flow, DeviceState.ACTIVE);
                    uint deviceCount = devices.GetCount();
                    for (uint i = 0; i < deviceCount; i++)
                    {
                        ((IMMNotificationClient)this).OnDeviceAdded(devices.Item(i).GetId());
                    }

                    _dispatcher.Invoke((Action)(() =>
                    {
                        QueryDefaultDevice();
                        Loaded?.Invoke(this, null);
                    }));
                }
                catch (Exception ex)
                {
                    // Even through we're going to be broken, show the tray icon so the user can collect debug data.
                    AppTrace.LogWarning(ex);

                    _dispatcher.Invoke((Action)(() =>
                    {
                        Loaded?.Invoke(this, null);
                    }));
                }
            });

            TraceLine($"Create Exit");
        }

        ~AudioDeviceManager()
        {
            try
            {
                _enumerator.UnregisterEndpointNotificationCallback(this);
            }
            catch (Exception ex)
            {
                TraceLine($"{ex}");
            }
        }

        public IAudioDevice Default => _default;

        private void QueryDefaultDevice()
        {
            TraceLine("QueryDefaultDevice");
            var currentDeviceId = _default?.Id;
            _default = GetDefaultDevice();
            string newDeviceId = _default?.Id;
            if (currentDeviceId != newDeviceId)
            {
                DefaultChanged?.Invoke(this, _default);
            }
        }

        public void SetDefaultDevice(IAudioDevice device, ERole role = ERole.eMultimedia)
        {
            TraceLine($"SetDefaultDevice {device.Id}");

            if (s_PolicyConfigClient == null)
            {
                s_PolicyConfigClient = new AutoPolicyConfigClientWin7();
            }

            // Racing with the system, the device may not be valid anymore.
            try
            {
                s_PolicyConfigClient.SetDefaultEndpoint(device.Id, role);
            }
            catch (Exception ex)
            {
                TraceLine($"{ex}");
            }
        }

        public IAudioDevice GetDefaultDevice(ERole eRole = ERole.eMultimedia)
        {
            IMMDevice device = null;
            try
            {
                device = _enumerator.GetDefaultAudioEndpoint(Flow, ERole.eMultimedia);
            }
            catch (Exception ex) when (ex.Is(HRESULT.ERROR_NOT_FOUND))
            {
                // Expected.
            }

            _devices.TryFind(device.GetId(), out var dev);
            return dev;
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
            TraceLine($"OnDeviceAdded {pwstrDeviceId}");

            if (!_devices.TryFind(pwstrDeviceId, out IAudioDevice unused))
            {
                try
                {
                    IMMDevice device = _enumerator.GetDevice(pwstrDeviceId);
                    if (((IMMEndpoint)device).GetDataFlow() == Flow)
                    {
                        var newDevice = new AudioDevice(this, device);

                        _dispatcher.Invoke((Action)(() =>
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
                    TraceLine($"{ex}");
                }
            }
        }

        void IMMNotificationClient.OnDeviceRemoved(string pwstrDeviceId)
        {
            TraceLine($"OnDeviceRemoved {pwstrDeviceId}");

            _dispatcher.Invoke((Action)(() =>
            {
                if (_devices.TryFind(pwstrDeviceId, out IAudioDevice dev))
                {
                    _devices.Remove(dev);
                }
            }));
        }

        void IMMNotificationClient.OnDefaultDeviceChanged(EDataFlow flow, ERole role, string pwstrDefaultDeviceId)
        {
            if (flow == Flow)
            {
                TraceLine($"OnDefaultDeviceChanged {pwstrDefaultDeviceId}");

                _dispatcher.Invoke((Action)(() =>
                {
                    QueryDefaultDevice();
                }));
            }
        }

        void IMMNotificationClient.OnDeviceStateChanged(string pwstrDeviceId, DeviceState dwNewState)
        {
            TraceLine($"OnDeviceStateChanged {pwstrDeviceId} {dwNewState}");
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
                    TraceLine($"Unknown DEVICE_STATE: {dwNewState}");
                    break;
            }
        }

        void IMMNotificationClient.OnPropertyValueChanged(string pwstrDeviceId, PROPERTYKEY key)
        {
            TraceLine($"OnPropertyValueChanged {pwstrDeviceId} {key.fmtid},{key.pid}");
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
                        TraceLine($"{ex}");
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

        private void TraceLine(string message)
        {
            System.Diagnostics.Trace.WriteLine($"AudioDeviceManager-({_kind}) {message}");
        }
    }
}
