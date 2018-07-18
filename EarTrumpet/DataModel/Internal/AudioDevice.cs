using EarTrumpet.Extensions;
using EarTrumpet.Interop;
using EarTrumpet.Interop.MMDeviceAPI;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Threading;

namespace EarTrumpet.DataModel.Internal
{
    class AudioDevice : IAudioEndpointVolumeCallback, IAudioDevice
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly Dispatcher _dispatcher;
        private readonly IAudioEndpointVolume _deviceVolume;
        private readonly AudioDeviceSessionCollection _sessions;
        private readonly IAudioMeterInformation _meter;
        private readonly WeakReference<IAudioDeviceManager> _deviceManager;
        private readonly string _id;
        private IMMDevice _device;
        private string _displayName;
        private float _volume;
        private bool _isMuted;
        private bool _isRegistered;

        public AudioDevice(IAudioDeviceManager deviceManager, IMMDevice device)
        {
            _device = device;
            _deviceManager = new WeakReference<IAudioDeviceManager>(deviceManager);
            _dispatcher = App.Current.Dispatcher;
            _id = device.GetId();

            Trace.WriteLine($"AudioDevice Create {_id}");

            _deviceVolume = device.Activate<IAudioEndpointVolume>();

            _deviceVolume.RegisterControlChangeNotify(this);
            _isRegistered = true;
            _meter = device.Activate<IAudioMeterInformation>();
            _sessions = new AudioDeviceSessionCollection(this, _device);

            _deviceVolume.GetMasterVolumeLevelScalar(out _volume);
            _isMuted = _deviceVolume.GetMute() != 0;

            ReadDisplayName();
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
                Trace.TraceError($"{ex}");
            }
        }

        void IAudioEndpointVolumeCallback.OnNotify(ref AUDIO_VOLUME_NOTIFICATION_DATA pNotify)
        {
            _volume = pNotify.fMasterVolume;
            _isMuted = pNotify.bMuted != 0;

            _dispatcher.BeginInvoke((Action)(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Volume)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsMuted)));
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
                    catch (Exception ex) when (ex.Is(Error.AUDCLNT_E_DEVICE_INVALIDATED))
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
                    catch (Exception ex) when (ex.Is(Error.AUDCLNT_E_DEVICE_INVALIDATED))
                    {
                        // Expected in some cases.
                    }
                }
            }
        }

        public string Id => _id;

        public ObservableCollection<IAudioDeviceSession> Groups => _sessions.Sessions;

        public string DisplayName => _displayName;

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

        public void UpdatePeakValueBackground()
        {
            var newValues = Helpers.ReadPeakValues(_meter);
            PeakValue1 = newValues[0];
            PeakValue2 = newValues[1];
        }

        public void UnhideSessionsForProcessId(int processId)
        {
            _sessions.UnHideSessionsForProcessId(processId);
        }

        public void MoveHiddenAppsToDevice(string appId, string id)
        {
            _sessions.MoveHiddenAppsToDevice(appId, id);
        }

        private void ReadDisplayName()
        {
            try
            {
                var propStore = _device.OpenPropertyStore(STGM.STGM_READ);
                var pv = propStore.GetValue(ref PropertyKeys.PKEY_Device_FriendlyName);
                _displayName = Marshal.PtrToStringUni(pv.union.pwszVal);
                Ole32.PropVariantClear(ref pv);
            }
            catch (Exception ex) when (ex.Is(Error.AUDCLNT_E_DEVICE_INVALIDATED))
            {
                // Expected in some cases.
            }
        }

        public void DevicePropertiesChanged(IMMDevice dev)
        {
            Trace.WriteLine($"AudioDevice DevicePropertiesChanged {_id}");

            _device = dev;
            ReadDisplayName();

            _dispatcher.BeginInvoke((Action)(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayName)));
            }));
        }
    }
}
