using EarTrumpet.DataModel.Audio;
using EarTrumpet.Extensions;
using EarTrumpet.Interop;
using EarTrumpet.Interop.MMDeviceAPI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Threading;

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
            _id = device.GetId();

            Trace.WriteLine($"AudioDevice Create {_id}");

            if (_device.GetState() == DeviceState.ACTIVE)
            {
                _deviceVolume = device.Activate<IAudioEndpointVolume>();
                _deviceVolume.RegisterControlChangeNotify(this);
                _deviceVolume.GetMasterVolumeLevelScalar(out _volume);
                _isMuted = _deviceVolume.GetMute() != 0;
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

        void IAudioEndpointVolumeCallback.OnNotify(IntPtr pNotify)
        {
            var data = Marshal.PtrToStructure<AUDIO_VOLUME_NOTIFICATION_DATA>(pNotify);
            _volume = data.fMasterVolume;
            _isMuted = data.bMuted != 0;

            _channels.OnNotify(pNotify, data);

            _dispatcher.Invoke((Action)(() =>
            {
                RaisePropertyChanged(nameof(Volume));
                RaisePropertyChanged(nameof(IsMuted));
            }));
        }

        public float Volume
        {
            get => _volume;
            set
            {
                value = value.Bound(0, 1f);

                if (_volume != value)
                {
                    try
                    {
                        _volume = value;
                        Guid dummy = Guid.Empty;
                        _deviceVolume.SetMasterVolumeLevelScalar(value, ref dummy);
                    }
                    catch (Exception ex) when (ex.Is(HRESULT.AUDCLNT_E_DEVICE_INVALIDATED))
                    {
                        // Expected in some cases.
                    }
                    IsMuted = _volume.ToVolumeInt() == 0;
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
                        Guid dummy = Guid.Empty;
                        _deviceVolume.SetMute(value ? 1 : 0, ref dummy);
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

        public void UnhideSessionsForProcessId(int processId)
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
                var propStore = _device.OpenPropertyStore(STGM.STGM_READ);
                _displayName = propStore.GetValue<string>(PropertyKeys.PKEY_Device_FriendlyName);
                _iconPath = propStore.GetValue<string>(PropertyKeys.DEVPKEY_DeviceClass_IconPath);
                _enumeratorName = propStore.GetValue<string>(PropertyKeys.DEVPKEY_Device_EnumeratorName);
                _interfaceName = propStore.GetValue<string>(PropertyKeys.DEVPKEY_DeviceInterface_FriendlyName);
                _deviceDescription = propStore.GetValue<string>(PropertyKeys.DEVPKEY_Device_DeviceDesc);
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
                var propStore = _device.OpenPropertyStore(STGM.STGM_READ);
                var pv = propStore.GetValue(ref PropertyKeys.PKEY_AudioEndpoint_PhysicalSpeakers);
                _speakerConfig = pv.uintVal;
                Ole32.PropVariantClear(ref pv);
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
