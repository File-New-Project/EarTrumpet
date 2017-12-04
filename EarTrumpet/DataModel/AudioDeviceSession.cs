using EarTrumpet.Extensions;
using EarTrumpet.DataModel.Com;
using System;
using System.ComponentModel;
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
        Dispatcher _dispatcher;
        string _processDisplayName;
        string _processIconPath;
        bool _isDesktopApp;
        uint _backgroundColor;
        bool _isHidden;

        public AudioDeviceSession(IAudioSessionControl session, Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            _session = session;
            _isHidden = _session.IsHidden();
            _meter = (IAudioMeterInformation)_session;
            _simpleVolume = (ISimpleAudioVolume)session;

            _session.RegisterAudioSessionNotification(this);

            Interop.GetProcessProperties(ProcessId, out _processDisplayName, out _processIconPath, ref _isDesktopApp, ref _backgroundColor);

            if (IsSystemSoundsSession)
            {
                _isDesktopApp = true;
            }

            if (_processDisplayName == null) _processDisplayName = "";
        }

        public float Volume
        {
            get
            {
                _simpleVolume.GetMasterVolume(out float level);
                return level;
            }
            set
            {
                Guid dummy = Guid.Empty;
                _simpleVolume.SetMasterVolume(value, ref dummy);
            }
        }

        public bool IsMuted
        {
            get
            {
                _simpleVolume.GetMute(out int muted);
                return muted != 0;
            }
            set
            {
                Guid dummy = Guid.Empty;
                _simpleVolume.SetMute(value ? 1 : 0, ref dummy);
            }
        }

        public bool IsHidden => _isHidden;

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

        public AudioSessionState State
        {
            get
            {
                _session.GetState(out AudioSessionState ret);
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

        public string Id
        {
            get
            {
                ((IAudioSessionControl2)_session).GetSessionInstanceIdentifier(out string ret);
                return ret;
            }
        }

        public bool IsSystemSoundsSession
        {
            get
            {
                return ((IAudioSessionControl2)_session).IsSystemSoundsSession() == 0;
            }
        }

        void IAudioSessionEvents.OnSimpleVolumeChanged(float NewVolume, int NewMute, ref Guid EventContext)
        {
            _dispatcher.SafeInvoke(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Volume"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsMuted"));
            });
        }

        void IAudioSessionEvents.OnGroupingParamChanged(ref Guid NewGroupingParam, ref Guid EventContext)
        {
            _dispatcher.SafeInvoke(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("GroupingParam"));
            });
        }

        void IAudioSessionEvents.OnStateChanged(AudioSessionState NewState)
        {
            _dispatcher.SafeInvoke(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("State"));
            });
        }

        // Unused.
        void IAudioSessionEvents.OnSessionDisconnected(AudioSessionDisconnectReason DisconnectReason){}
        void IAudioSessionEvents.OnChannelVolumeChanged(uint ChannelCount, ref float NewChannelVolumeArray, uint ChangedChannel, ref Guid EventContext){}
        void IAudioSessionEvents.OnDisplayNameChanged(string NewDisplayName, ref Guid EventContext) { }
        void IAudioSessionEvents.OnIconPathChanged(string NewIconPath, ref Guid EventContext) { }
    }
}
