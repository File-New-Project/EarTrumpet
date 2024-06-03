using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using EarTrumpet.DataModel.Audio;
using EarTrumpet.Extensions;
using EarTrumpet.Interop.MMDeviceAPI;
using Windows.Win32;
using Windows.Win32.Media.Audio;
using Windows.Win32.Media.Audio.Endpoints;
using Windows.Win32.System.Com;

namespace EarTrumpet.DataModel.WindowsAudio.Internal
{
    public class AudioDevice : BindableBase, IAudioEndpointVolumeCallback, IAudioDevice, IAudioDeviceInternal, IAudioDeviceWindowsAudio
    {
        private readonly Dispatcher _dispatcher;
        private readonly IAudioEndpointVolume _deviceVolume;
        private readonly AudioDeviceSessionCollection _sessions;
        private readonly FilteredCollectionChain<IAudioDeviceSession> _sessionFilter;
        private readonly IAudioMeterInformation _meter;
        private readonly WeakReference<IAudioDeviceManager> _deviceManager;
        private readonly string _id;
        private readonly AudioDeviceChannelCollection _channels;
        private IMMDevice _device;
        private string _displayName;
        private string _iconPath;
        private string _enumeratorName;
        private string _interfaceName;
        private string _deviceDescription;
        private float _volume;
        private bool _isMuted;
        private bool _isRegistered;
        private uint _speakerConfig;

        public AudioDevice(IAudioDeviceManager deviceManager, IMMDevice device, Dispatcher foregroundDispatcher)
        {
            _device = device;
            _deviceManager = new WeakReference<IAudioDeviceManager>(deviceManager);
            _dispatcher = foregroundDispatcher;
            device.GetId(out var deviceId);
            _id = deviceId.ToString();

            Trace.WriteLine($"AudioDevice Create {_id}");

            _device.GetState(out var deviceState);
            if (deviceState == DEVICE_STATE.DEVICE_STATE_ACTIVE)
            {
                _deviceVolume = device.Activate<IAudioEndpointVolume>();
                _deviceVolume.RegisterControlChangeNotify(this);
                _deviceVolume.GetMasterVolumeLevelScalar(out _volume);
                _deviceVolume.GetMute(out var isMuted);
                _isMuted = isMuted;
                _isRegistered = true;
                _meter = device.Activate<IAudioMeterInformation>();
                _channels = new AudioDeviceChannelCollection(_deviceVolume, _dispatcher);
                _sessions = new AudioDeviceSessionCollection(this, _device, _dispatcher);
                _sessionFilter = new FilteredCollectionChain<IAudioDeviceSession>(_sessions.Sessions, _dispatcher);
                Groups = _sessionFilter.Items;
            }
            else
            {
                Groups = new ObservableCollection<IAudioDeviceSession>();
            }

            ReadProperties();
        }

        ~AudioDevice()
        {
            try
            {
                if (_isRegistered)
                {
                    _deviceVolume.UnregisterControlChangeNotify(this);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"AudioDevice .dtor Failed {ex}");
            }
        }

        public unsafe void OnNotify(AUDIO_VOLUME_NOTIFICATION_DATA* pNotify)
        {
            _volume = (*pNotify).fMasterVolume;
            _isMuted = (*pNotify).bMuted != 0;

            _channels.OnNotify((nint)pNotify, *pNotify);

            _dispatcher.Invoke((Action)(() =>
            {
                RaisePropertyChanged(nameof(Volume));
                RaisePropertyChanged(nameof(IsMuted));
            }));
        }

        public float Volume
        {
            get => App.Settings.UseLogarithmicVolume ? _volume.ToDisplayVolume() : _volume;
            set
            {
                value = value.Bound(0, 1f);

                if (App.Settings.UseLogarithmicVolume)
                {
                    value = value.ToLogVolume();
                }

                if (_volume != value)
                {
                    try
                    {
                        _volume = value;
                        _deviceVolume.SetMasterVolumeLevelScalar(value, Guid.Empty);
                    }
                    catch (Exception ex) when (ex.Is(HRESULT.AUDCLNT_E_DEVICE_INVALIDATED))
                    {
                        // Expected in some cases.
                    }

                    IsMuted = App.Settings.UseLogarithmicVolume ? _volume <= (1 / 100f).ToLogVolume() : _volume.ToVolumeInt() == 0;
                }
            }
        }

        public float PeakValue1 { get; private set; }
        public float PeakValue2 { get; private set; }

        public bool IsMuted
        {
            get => _isMuted;
            set
            {
                if (value != _isMuted)
                {
                    try
                    {
                        _deviceVolume.SetMute(value ? new BOOL(1) : new BOOL(0), Guid.Empty);
                    }
                    catch (Exception ex) when (ex.Is(HRESULT.AUDCLNT_E_DEVICE_INVALIDATED))
                    {
                        // Expected in some cases.
                    }
                }
            }
        }

        public string Id => _id;

        public ObservableCollection<IAudioDeviceSession> Groups { get; }

        public string DisplayName => _displayName;
        public string IconPath => _iconPath;
        public string EnumeratorName => _enumeratorName;
        public string InterfaceName => _interfaceName;
        public string DeviceDescription => _deviceDescription;

        public IAudioDeviceManager Parent
        {
            get
            {
                if (_deviceManager.TryGetTarget(out var parent))
                {
                    return parent;
                }
                return null;
            }
        }

        public IEnumerable<IAudioDeviceChannel> Channels => _channels.Channels;

        public void UpdatePeakValue()
        {
            var newValues = Helpers.ReadPeakValues(_meter);
            PeakValue1 = newValues[0];
            PeakValue2 = newValues[1];

            foreach(var session in _sessions.Sessions.ToArray())
            {
                ((IAudioDeviceSessionInternal)session).UpdatePeakValueBackground();
            }
        }

        public void UnhideSessionsForProcessId(uint processId)
        {
            _sessions.UnHideSessionsForProcessId(processId);
        }

        public void MoveHiddenAppsToDevice(string appId, string id)
        {
            _sessions.MoveHiddenAppsToDevice(appId, id);
        }

        private void ReadProperties()
        {
            try
            {
                _device.OpenPropertyStore(STGM.STGM_READ, out var propStore);
                _displayName = propStore.GetValue<string>(PInvoke.PKEY_Device_FriendlyName);
                _iconPath = propStore.GetValue<string>(PInvoke.DEVPKEY_DeviceClass_IconPath);
                _enumeratorName = propStore.GetValue<string>(PInvoke.DEVPKEY_Device_EnumeratorName);
                _interfaceName = propStore.GetValue<string>(PInvoke.DEVPKEY_DeviceInterface_FriendlyName);
                _deviceDescription = propStore.GetValue<string>(PInvoke.DEVPKEY_Device_DeviceDesc);
            }
            catch (Exception ex) when (ex.Is(HRESULT.AUDCLNT_E_DEVICE_INVALIDATED))
            {
                // Expected in some cases.
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }

        private void ReadSpeakerConfiguration()
        {
            try
            {
                _device.OpenPropertyStore(STGM.STGM_READ, out var propStore);
                propStore.GetValue(PInvoke.PKEY_AudioEndpoint_PhysicalSpeakers, out var pv);
                _speakerConfig = pv.Anonymous.Anonymous.Anonymous.uintVal;
                PInvoke.PropVariantClear(ref pv);
            }
            catch (Exception ex) when (ex.Is(HRESULT.AUDCLNT_E_DEVICE_INVALIDATED))
            {
                // Expected in some cases.
            }
        }

        public void DevicePropertiesChanged(IMMDevice dev)
        {
            Trace.WriteLine($"AudioDevice DevicePropertiesChanged {_id}");

            _device = dev;
            ReadProperties();

            _dispatcher.Invoke((Action)(() =>
            {
                RaisePropertyChanged(nameof(DisplayName));
            }));
        }

        public void AddFilter(Func<ObservableCollection<IAudioDeviceSession>, ObservableCollection<IAudioDeviceSession>> filter)
        {
            _sessionFilter.AddFilter(filter);
        }
    }
}
