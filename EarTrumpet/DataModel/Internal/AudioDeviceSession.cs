using EarTrumpet.DataModel.Internal.Services;
using EarTrumpet.Extensions;
using EarTrumpet.Interop.MMDeviceAPI;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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

        public int ProcessId { get; }

        public string Id => _id;

        public bool IsSystemSoundsSession => ((IAudioSessionControl2)_session).IsSystemSoundsSession() == 0;

        public string PersistedDefaultEndPointId => AudioPolicyConfigService.GetDefaultEndPoint(ProcessId);

        public ObservableCollection<IAudioDeviceSession> Children { get; private set; }

        private readonly string _id;
        private readonly IAudioSessionControl _session;
        private readonly ISimpleAudioVolume _simpleVolume;
        private readonly IAudioMeterInformation _meter;
        private readonly Dispatcher _dispatcher;
        private readonly AppInformation _appInfo;

        private string _resolvedDisplayName;
        private float _volume;
        private bool _isMuted;
        private bool _isDisconnected;
        private bool _isMoved;
        private Task _refreshDisplayNameTask;

        public AudioDeviceSession(IAudioSessionControl session)
        {
            _dispatcher = App.Current.Dispatcher;
            _session = session;
            _meter = (IAudioMeterInformation)_session;
            _simpleVolume = (ISimpleAudioVolume)session;
            ProcessId = (int)((IAudioSessionControl2)_session).GetProcessId();

            _session.RegisterAudioSessionNotification(this);
            ((IAudioSessionControl2)_session).GetSessionInstanceIdentifier(out _id);

            Trace.WriteLine($"AudioDeviceSession Create {_id}");

            _appInfo = AppInformationService.GetInformationForAppByPid(ProcessId);

            if (!IsSystemSoundsSession)
            {
                ProcessWatcherService.WatchProcess(ProcessId, (pid) => DisconnectSession());
            }

            ReadVolumeAndMute();
            RefreshDisplayName();
        }

        public void RefreshDisplayName()
        {
            Trace.WriteLine($"AudioDeviceSession RefreshDisplayName {Id}");
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

                    Task.Delay(TimeSpan.FromSeconds(5)).Wait();
                });

                _refreshDisplayNameTask.Start();
            }
        }

        public void MoveFromDevice()
        {
            Trace.WriteLine($"AudioDeviceSession MoveFromDevice {Id}");
            _isMoved = true;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(State)));
        }

        public void MoveAllSessionsToDevice(string id)
        {
            // The group should have handled this.
            throw new NotImplementedException();
        }

        private void ReadVolumeAndMute()
        {
            _simpleVolume.GetMasterVolume(out _volume);
            _isMuted = _simpleVolume.GetMute() != 0;
        }

        private void DisconnectSession()
        {
            Trace.WriteLine($"AudioDeviceSession DisconnectSession {Id}"); 

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
            Trace.WriteLine($"AudioDeviceSession OnGroupingParamChanged {Id}");
            _dispatcher.SafeInvoke(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GroupingParam)));
            });
        }

        void IAudioSessionEvents.OnStateChanged(AudioSessionState NewState)
        {
            Trace.WriteLine($"AudioDeviceSession OnStateChanged {NewState} {Id}");
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
