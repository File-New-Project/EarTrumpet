using EarTrumpet.Extensions;
using EarTrumpet.Interop.MMDeviceAPI;
using EarTrumpet.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Threading;

namespace EarTrumpet.DataModel.Internal
{
    public class AudioDeviceSession : IAudioSessionEvents, IAudioDeviceSession
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly IAudioSessionControl _session;
        private readonly ISimpleAudioVolume _simpleVolume;
        private readonly IAudioMeterInformation _meter;
        private readonly Dispatcher _dispatcher;
        private readonly AppInformation _appInfo;
        private string _resolvedDisplayName;
        private string _id;
        private float _volume;
        private bool _isMuted;
        private bool _isDisconnected;

        public AudioDeviceSession(IAudioSessionControl session, IAudioDevice device, Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            Device = device;
            _session = session;
            _meter = (IAudioMeterInformation)_session;
            _simpleVolume = (ISimpleAudioVolume)session;
            _session.RegisterAudioSessionNotification(this);
            ((IAudioSessionControl2)_session).GetSessionInstanceIdentifier(out _id);

            _appInfo = AppInformationService.GetInformationForAppByPid(ProcessId,
                (displayName) =>
                {
                    dispatcher.SafeInvoke(() =>
                    {
                        _resolvedDisplayName = displayName;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayName)));
                    });
                });

            ReadVolumeAndMute();
        }

        public IAudioDevice Device { get; }

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

                    if (IsMuted)
                    {
                        IsMuted = false;
                    }
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

        public string DisplayName => !string.IsNullOrWhiteSpace(_resolvedDisplayName) ? _resolvedDisplayName : _appInfo.ExeName;

        public string ExeName => _appInfo.ExeName;

        public string IconPath => _appInfo.SmallLogoPath;

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

        public uint BackgroundColor => _appInfo.BackgroundColor;

        public bool IsDesktopApp => _appInfo.IsDesktopApp;

        public string AppId => _appInfo.PackageInstallPath;

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

        public ObservableCollection<IAudioDeviceSession> Children { get; private set; }

        private void ReadVolumeAndMute()
        {
            try
            {
                _simpleVolume.GetMasterVolume(out _volume);
                _simpleVolume.GetMute(out int muted);
                _isMuted = muted != 0;
            }
            catch(COMException ex)
            {
                Debug.WriteLine(ex);
            }
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
