using EarTrumpet.DataModel.Com;
using EarTrumpet.Extensions;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Threading;

namespace EarTrumpet.DataModel
{
    public class AudioDeviceSession : IAudioSessionEvents, IAudioDeviceSession
    {
        static class Interop
        {
            [DllImport("EarTrumpet.Interop.dll")]
            public static extern int GetProcessProperties(int processId,
                 [MarshalAs(UnmanagedType.LPWStr)] out string displayName,
                 [MarshalAs(UnmanagedType.LPWStr)] out string iconPath,
                 [MarshalAs(UnmanagedType.Bool)] ref bool isDesktopApp,
                 [MarshalAs(UnmanagedType.U4)] ref uint backgroundColor);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        IAudioSessionControl _session;
        ISimpleAudioVolume _simpleVolume;
        IAudioMeterInformation _meter;
        IAudioDevice _device;
        Dispatcher _dispatcher;
        string _processDisplayName;
        string _processIconPath;
        string _appId;
        string _id;
        float _volume;
        bool _isMuted;
        bool _isDesktopApp;
        bool _isDisconnected;
        uint _backgroundColor;

        public AudioDeviceSession(IAudioSessionControl session, IAudioDevice device, Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            _device = device;
            _session = session;
            _meter = (IAudioMeterInformation)_session;
            _simpleVolume = (ISimpleAudioVolume)session;

            _session.RegisterAudioSessionNotification(this);

            ((IAudioSessionControl2)_session).GetSessionInstanceIdentifier(out _id);

            Interop.GetProcessProperties(ProcessId, out _processDisplayName, out _processIconPath, ref _isDesktopApp, ref _backgroundColor);

            if (_processDisplayName == null)
            {
                _processDisplayName = "";
            }

            if (IsSystemSoundsSession)
            {
                _isDesktopApp = true;
            }
            else
            {
                try
                {
                    _appId = Process.GetProcessById(ProcessId).GetMainModuleFileName();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    // Our process could have quit and we haven't seen the notification yet, so don't crash.
                }
            }

            ReadVolumeAndMute();
        }

        public IAudioDevice Device => _device;

        public float Volume
        {
            get => _volume;
            set
            {
                if (value < 0)
                {
                    value = 0;
                }

                if (value > 1.0f)
                {
                    value = 1.0f;
                }

                if (value != _volume)
                {
                    Guid dummy = Guid.Empty;
                    _simpleVolume.SetMasterVolume(value, ref dummy);
                }

            }
        }

        public bool IsMuted
        {
            get => _isMuted;
            set
            {
                if (value != _isMuted)
                {
                    Guid dummy = Guid.Empty;
                    _simpleVolume.SetMute(value ? 1 : 0, ref dummy);
                }
            }
        }

        public string DisplayName => _processDisplayName;

        public string IconPath
        {
            get
            {
                if (IsSystemSoundsSession)
                {
                    return Environment.ExpandEnvironmentVariables(
                        $"%windir%\\{(Environment.Is64BitOperatingSystem ? "sysnative" : "system32")}\\audiosrv.dll");
                }
                else
                {
                    return _processIconPath;
                }
            }
        }

        public Guid GroupingParam
        {
            get
            {
                _session.GetGroupingParam(out Guid ret);
                return ret;
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

        public uint BackgroundColor => _backgroundColor;

        public bool IsDesktopApp => _isDesktopApp;

        public string AppId => _appId;

        public AudioSessionState State
        {
            get
            {
                if (_isDisconnected)
                {
                    return AudioSessionState.Expired;
                }
                _session.GetState(out AudioSessionState ret);
                return ret;
            }
        }

        public string RawDisplayName
        {
            get
            {
                _session.GetDisplayName(out string ret);
                return ret;
            }
        }

        public int ProcessId
        {
            get
            {
                ((IAudioSessionControl2)_session).GetProcessId(out uint ret);
                return (int)ret;
            }
        }

        public string Id => _id;

        public bool IsSystemSoundsSession
        {
            get
            {
                return ((IAudioSessionControl2)_session).IsSystemSoundsSession() == 0;
            }
        }

        public ObservableCollection<IAudioDeviceSession> Children => null;

        void ReadVolumeAndMute()
        {
            _simpleVolume.GetMasterVolume(out _volume);
            _simpleVolume.GetMute(out int muted);
            _isMuted = muted != 0;
        }

        void IAudioSessionEvents.OnSimpleVolumeChanged(float NewVolume, int NewMute, ref Guid EventContext)
        {
            ReadVolumeAndMute();

            _dispatcher.SafeInvoke(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Volume)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsMuted)));
            });
        }

        void IAudioSessionEvents.OnGroupingParamChanged(ref Guid NewGroupingParam, ref Guid EventContext)
        {
            _dispatcher.SafeInvoke(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GroupingParam)));
            });
        }

        void IAudioSessionEvents.OnStateChanged(AudioSessionState NewState)
        {
            _dispatcher.SafeInvoke(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(State)));
            });
        }

        void IAudioSessionEvents.OnSessionDisconnected(AudioSessionDisconnectReason DisconnectReason)
        {
            _isDisconnected = true;
            _dispatcher.SafeInvoke(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(State)));
            });
        }

        void IAudioSessionEvents.OnChannelVolumeChanged(uint ChannelCount, ref float NewChannelVolumeArray, uint ChangedChannel, ref Guid EventContext){}
        void IAudioSessionEvents.OnDisplayNameChanged(string NewDisplayName, ref Guid EventContext) { }
        void IAudioSessionEvents.OnIconPathChanged(string NewIconPath, ref Guid EventContext) { }
    }
}
