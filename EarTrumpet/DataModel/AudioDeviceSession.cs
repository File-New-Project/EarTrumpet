using EarTrumpet.DataModel.Interfaces;
using EarTrumpet.Extensions;
using SoundControlAPI_Interop;
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

        IAudioSessionControl m_session;
        ISimpleAudioVolume m_simpleVolume;
        IAudioMeterInformation m_meter;
        Dispatcher m_dispatcher;
        string m_processDisplayName;
        string m_processIconPath;
        bool m_isDesktopApp;
        uint m_backgroundColor;
        bool m_isHidden;

        static readonly int AUDCLNT_SESSIONFLAGS_DISPLAY_HIDE = 0x20000000;

        public AudioDeviceSession(IAudioSessionControl session, Dispatcher dispatcher)
        {
            m_dispatcher = dispatcher;
            m_session = session;
            m_meter = (IAudioMeterInformation)m_session;
            m_simpleVolume = (ISimpleAudioVolume)session;

            m_session.RegisterAudioSessionNotification(this);

            ((IAudioSessionControlInternal)m_session).GetStreamFlags(out int flags);

            m_isHidden = (flags & AUDCLNT_SESSIONFLAGS_DISPLAY_HIDE) == AUDCLNT_SESSIONFLAGS_DISPLAY_HIDE;

            Interop.GetProcessProperties(ProcessId, out m_processDisplayName, out m_processIconPath, ref m_isDesktopApp, ref m_backgroundColor);

            if (IsSystemSoundsSession)
            {
                m_isDesktopApp = true;
            }
        }

        public float Volume
        {
            get
            {
                m_simpleVolume.GetMasterVolume(out float level);
                return level;
            }
            set
            {
                Guid dummy = Guid.Empty;
                m_simpleVolume.SetMasterVolume(value, ref dummy);
            }
        }

        public bool IsMuted
        {
            get
            {
                m_simpleVolume.GetMute(out int muted);
                return muted != 0;
            }
            set
            {
                Guid dummy = Guid.Empty;
                m_simpleVolume.SetMute(value ? 1 : 0, ref dummy);
            }
        }

        public bool IsHidden
        {
            get
            {
                return m_isHidden;
            }
        }

        public string DisplayName
        {
            get
            {
                if (IsSystemSoundsSession)
                {
                    return "System sounds"; // TODO factor out of upper stack
                }
                else
                {
                    return m_processDisplayName;
                }
            }
        }

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
                    return m_processIconPath;
                }
            }
        }

        public Guid GroupingParam
        {
            get
            {
                m_session.GetGroupingParam(out Guid ret);
                return ret;
            }
        }

        public float PeakValue
        {
            get
            {
                m_meter.GetMeteringChannelCount(out uint count);

                m_meter.GetPeakValue(out float ret);
                return ret;
            }
        }

        public uint BackgroundColor => m_backgroundColor;

        public bool IsDesktopApp => m_isDesktopApp;

        public _AudioSessionState State
        {
            get
            {
                m_session.GetState(out _AudioSessionState ret);
                return ret;
            }
        }

        public int ProcessId
        {
            get
            {
                ((IAudioSessionControl2)m_session).GetProcessId(out uint ret);
                return (int)ret;
            }
        }

        public string Id
        {
            get
            {
                ((IAudioSessionControl2)m_session).GetSessionInstanceIdentifier(out string ret);
                return ret;
            }
        }

        public bool IsSystemSoundsSession => ((IAudioSessionControl2)m_session).GetIsSystemSoundsSession();

        void IAudioSessionEvents.OnSimpleVolumeChanged(float NewVolume, int NewMute, ref Guid EventContext)
        {
            m_dispatcher.SafeInvoke(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Volume"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsMuted"));
            });
        }

        void IAudioSessionEvents.OnGroupingParamChanged(ref Guid NewGroupingParam, ref Guid EventContext)
        {
            m_dispatcher.SafeInvoke(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("GroupingParam"));
            });
        }

        void IAudioSessionEvents.OnStateChanged(_AudioSessionState NewState)
        {
            m_dispatcher.SafeInvoke(() =>
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
