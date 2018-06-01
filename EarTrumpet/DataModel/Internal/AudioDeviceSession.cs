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
    class AudioDeviceSession : IAudioSessionEvents, IAudioDeviceSession
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public float Volume
        {
            get => _volume;
            set
            {
                value = value.Bound(0, 1f);

                if (value != _volume)
                {
                    Guid dummy = Guid.Empty;
                    _simpleVolume.SetMasterVolume(value, ref dummy);
                    IsMuted = false;
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

        public Guid GroupingParam => _session.GetGroupingParam();

        public float PeakValue => _meter.GetPeakValue();

        public uint BackgroundColor => _appInfo.BackgroundColor;

        public bool IsDesktopApp => _appInfo.IsDesktopApp;

        public string AppId => _appInfo.PackageInstallPath;

        public SessionState State
        {
            get
            {
                if (_isDisconnected)
                {
                    return SessionState.Expired;
                }
                else if (_isMoved)
                {
                    return SessionState.Moved;
                }

                switch (_session.GetState())
                {
                    case AudioSessionState.Active:
                        return SessionState.Active;
                    case AudioSessionState.Inactive:
                        return SessionState.Inactive;
                    case AudioSessionState.Expired:
                        return SessionState.Expired;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public string RawDisplayName => _session.GetDisplayName();

        public int ProcessId => (int)((IAudioSessionControl2)_session).GetProcessId();

        public string Id => _id;

        public bool IsSystemSoundsSession => ((IAudioSessionControl2)_session).IsSystemSoundsSession() == 0;

        public ObservableCollection<IAudioDeviceSession> Children { get; private set; }

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
        private bool _isMoved;

        public AudioDeviceSession(IAudioSessionControl session)
        {
            _dispatcher = App.Current.Dispatcher;
            _session = session;
            _meter = (IAudioMeterInformation)_session;
            _simpleVolume = (ISimpleAudioVolume)session;
            _session.RegisterAudioSessionNotification(this);
            ((IAudioSessionControl2)_session).GetSessionInstanceIdentifier(out _id);

            _appInfo = AppInformationService.GetInformationForAppByPid(ProcessId,
                (displayName) =>
                {
                    _dispatcher.SafeInvoke(() =>
                    {
                        _resolvedDisplayName = displayName;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayName)));
                    });
                });

            if (!IsSystemSoundsSession)
            {
                ProcessWatcherService.WatchProcess(ProcessId, (pid) => DisconnectSession());
            }

            ReadVolumeAndMute();
        }

        public void MoveFromDevice()
        {
            _isMoved = true;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(State)));
        }

        private void ReadVolumeAndMute()
        {
            try
            {
                _simpleVolume.GetMasterVolume(out _volume);
                _isMuted = _simpleVolume.GetMute() != 0;
            }
            catch(COMException ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private void DisconnectSession()
        {
            _isDisconnected = true;
            _dispatcher.SafeInvoke(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(State)));
            });
        }

        void IAudioSessionEvents.OnSimpleVolumeChanged(float NewVolume, int NewMute, ref Guid EventContext)
        {
            // We're racing and the device might be dead, so don't fail below.
            try
            {
                ReadVolumeAndMute();
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex);
            }

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
            if (_isMoved)
            {
                if (NewState == AudioSessionState.Active)
                {
                    _isMoved = false;
                }
            }

            _dispatcher.SafeInvoke(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(State)));
            });
        }

        void IAudioSessionEvents.OnSessionDisconnected(AudioSessionDisconnectReason DisconnectReason) => DisconnectSession();

        void IAudioSessionEvents.OnChannelVolumeChanged(uint ChannelCount, ref float NewChannelVolumeArray, uint ChangedChannel, ref Guid EventContext){}
        void IAudioSessionEvents.OnDisplayNameChanged(string NewDisplayName, ref Guid EventContext) { }
        void IAudioSessionEvents.OnIconPathChanged(string NewIconPath, ref Guid EventContext) { }
    }
}
