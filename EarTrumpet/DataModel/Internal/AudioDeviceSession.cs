using EarTrumpet.DataModel.Internal.Services;
using EarTrumpet.Extensions;
using EarTrumpet.Interop;
using EarTrumpet.Interop.MMDeviceAPI;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
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

        public string DisplayName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_rawDisplayName))
                {
                    return _rawDisplayName;
                }
                else if (!string.IsNullOrWhiteSpace(_resolvedDisplayName))
                {
                    return _resolvedDisplayName;
                }
                else
                {
                    return _appInfo.ExeName;
                }
            }
        }

        public string ExeName => _appInfo.ExeName;

        public string IconPath => _appInfo.SmallLogoPath;

        public Guid GroupingParam { get; private set; }

        public float PeakValue { get; private set; }

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

                switch (_state)
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

        public int ProcessId { get; }

        public string Id => _id;

        public bool IsSystemSoundsSession { get; }

        public string PersistedDefaultEndPointId => AudioPolicyConfigService.GetDefaultEndPoint(ProcessId);

        public ObservableCollection<IAudioDeviceSession> Children { get; private set; }

        private readonly string _id;
        private readonly IAudioSessionControl _session;
        private readonly ISimpleAudioVolume _simpleVolume;
        private readonly IAudioMeterInformation _meter;
        private readonly Dispatcher _dispatcher;
        private readonly AppInformation _appInfo;

        private string _resolvedDisplayName;
        private string _rawDisplayName;
        private float _volume;
        private AudioSessionState _state;
        private bool _isMuted;
        private bool _isDisconnected;
        private bool _isMoved;
        private bool _moveOnInactive;
        private Task _refreshDisplayNameTask;

        public AudioDeviceSession(IAudioSessionControl session)
        {
            _dispatcher = App.Current.Dispatcher;
            _session = session;
            _meter = (IAudioMeterInformation)_session;
            _simpleVolume = (ISimpleAudioVolume)session;
            ProcessId = (int)((IAudioSessionControl2)_session).GetProcessId();
            IsSystemSoundsSession = ((IAudioSessionControl2)_session).IsSystemSoundsSession() == 0;
            _state = _session.GetState();
            GroupingParam = _session.GetGroupingParam();
            _simpleVolume.GetMasterVolume(out _volume);
            _isMuted = _simpleVolume.GetMute() != 0;

            _appInfo = AppInformationService.GetInformationForAppByPid(ProcessId);

            // NOTE: Ensure that the callbacks won't touch state that isn't initialized yet (i.e. _appInfo must be valid before the first callback)
            _session.RegisterAudioSessionNotification(this);
            ((IAudioSessionControl2)_session).GetSessionInstanceIdentifier(out _id);

            Trace.WriteLine($"AudioDeviceSession Create {ExeName} {_id}");

            if (_appInfo.CanTrack)
            {
                ProcessWatcherService.WatchProcess(ProcessId, (pid) => DisconnectSession());
            }

            ReadRawDisplayName();
            RefreshDisplayName();
        }

        ~AudioDeviceSession()
        {
            try
            {
                _session.UnregisterAudioSessionNotification(this);
            }
            catch (Exception ex)
            {
                Trace.TraceError($"{ex}");
            }
        }

        public void RefreshDisplayName()
        {
            if (_refreshDisplayNameTask == null || _refreshDisplayNameTask.IsCompleted)
            {
                _refreshDisplayNameTask = new Task(() =>
                {
                    var displayName = AppInformationService.GetDisplayNameForAppByPid(ProcessId);
                    _dispatcher.SafeInvoke(() =>
                    {
                        _resolvedDisplayName = displayName;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayName)));
                    });

                    // Add a short delay to avoid running the task over and over.
                    Task.Delay(TimeSpan.FromSeconds(5)).Wait();
                });

                _refreshDisplayNameTask.Start();
            }
        }

        public void MoveFromDevice()
        {
            Trace.WriteLine($"AudioDeviceSession MoveFromDevice {ExeName} {Id}");

            if (_state == AudioSessionState.Active)
            {
                _moveOnInactive = true;
            }
            else
            {
                _isMoved = true;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(State)));
            }
        }

        public void MoveAllSessionsToDevice(string id)
        {
            // The group should have handled this.
            Debug.Assert(false);
        }

        public void UpdatePeakValue()
        {
            PeakValue = _meter.GetPeakValue();
        }

        private void ReadRawDisplayName()
        {
            var displayName = _session.GetDisplayName();
            if (displayName.StartsWith("@"))
            {
                StringBuilder sb = new StringBuilder(512);
                if (Shlwapi.SHLoadIndirectString(displayName, sb, sb.Capacity, IntPtr.Zero) == 0)
                {
                    displayName = sb.ToString();
                }
            }

            _rawDisplayName = displayName;
        }

        private void DisconnectSession()
        {
            Trace.WriteLine($"AudioDeviceSession DisconnectSession {ExeName} {Id}");

            _isDisconnected = true;
            _dispatcher.SafeInvoke(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(State)));
            });
        }

        void IAudioSessionEvents.OnSimpleVolumeChanged(float NewVolume, int NewMute, ref Guid EventContext)
        {
            _volume = NewVolume;
            _isMuted = NewMute != 0;

            _dispatcher.SafeInvoke(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Volume)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsMuted)));
            });
        }

        void IAudioSessionEvents.OnGroupingParamChanged(ref Guid NewGroupingParam, ref Guid EventContext)
        {
            GroupingParam = NewGroupingParam;
            Trace.WriteLine($"AudioDeviceSession OnGroupingParamChanged {ExeName} {Id}");
            _dispatcher.SafeInvoke(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GroupingParam)));
            });
        }

        void IAudioSessionEvents.OnStateChanged(AudioSessionState NewState)
        {
            Trace.WriteLine($"AudioDeviceSession OnStateChanged {NewState} {ExeName} {Id}");

            _state = NewState;

            if (_isMoved && NewState == AudioSessionState.Active)
            {
                _isMoved = false;
            }
            else if (_moveOnInactive && NewState == AudioSessionState.Inactive)
            {
                _isMoved = true;
                _moveOnInactive = false;
            }

            _dispatcher.SafeInvoke(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(State)));
            });
        }

        void IAudioSessionEvents.OnDisplayNameChanged(string NewDisplayName, ref Guid EventContext)
        {
            ReadRawDisplayName();

            _dispatcher.SafeInvoke(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayName)));
            });
        }

        void IAudioSessionEvents.OnSessionDisconnected(AudioSessionDisconnectReason DisconnectReason) => DisconnectSession();

        void IAudioSessionEvents.OnChannelVolumeChanged(uint ChannelCount, ref float NewChannelVolumeArray, uint ChangedChannel, ref Guid EventContext) { }
        void IAudioSessionEvents.OnIconPathChanged(string NewIconPath, ref Guid EventContext) { }
    }
}
