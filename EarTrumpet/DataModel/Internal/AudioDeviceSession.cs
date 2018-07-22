using EarTrumpet.DataModel.Internal.Services;
using EarTrumpet.Extensions;
using EarTrumpet.Interop;
using EarTrumpet.Interop.MMDeviceAPI;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
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
                    try
                    {
                        _volume = value;
                        Guid dummy = Guid.Empty;
                        _simpleVolume.SetMasterVolume(value, ref dummy);
                    }
                    catch (Exception ex) when (ex.Is(Error.AUDCLNT_E_DEVICE_INVALIDATED))
                    {
                        // Expected in some cases.
                    }
                    IsMuted = _volume.ToVolumeInt() == 0;
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
                    try
                    {
                        Guid dummy = Guid.Empty;
                        _simpleVolume.SetMute(value ? 1 : 0, ref dummy);
                    }
                    catch (Exception ex) when (ex.Is(Error.AUDCLNT_E_DEVICE_INVALIDATED))
                    {
                        // Expected in some cases.
                    }
                }
            }
        }

        public string SessionDisplayName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_rawDisplayName))
                {
                    return _rawDisplayName;
                }
                else if (!string.IsNullOrWhiteSpace(_resolvedAppDisplayName))
                {
                    return _resolvedAppDisplayName;
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

        public float PeakValue1 { get; private set; }
        public float PeakValue2 { get; private set; }

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

        public string PersistedDefaultEndPointId
        {
            get
            {
                if (_parent.TryGetTarget(out var parent))
                {
                    return parent.Parent.GetDefaultEndPoint(ProcessId);
                }
                return null;
            }
        }

        public ObservableCollection<IAudioDeviceSession> Children { get; private set; }

        private readonly string _id;
        private readonly IAudioSessionControl _session;
        private readonly ISimpleAudioVolume _simpleVolume;
        private readonly IAudioMeterInformation _meter;
        private readonly Dispatcher _dispatcher;
        private readonly AppInformation _appInfo;

        private string _resolvedAppDisplayName;
        private string _rawDisplayName;
        private float _volume;
        private AudioSessionState _state;
        private bool _isMuted;
        private bool _isDisconnected;
        private bool _isMoved;
        private bool _moveOnInactive;
        private bool _isRegistered;
        private Task _refreshDisplayNameTask;
        private WeakReference<IAudioDevice> _parent;

        public AudioDeviceSession(IAudioDevice parent, IAudioSessionControl session)
        {
            _dispatcher = App.Current.Dispatcher;
            _session = session;
            _meter = (IAudioMeterInformation)_session;
            _simpleVolume = (ISimpleAudioVolume)session;
            _state = _session.GetState();
            GroupingParam = _session.GetGroupingParam();
            _simpleVolume.GetMasterVolume(out _volume);
            _isMuted = _simpleVolume.GetMute() != 0;
            IsSystemSoundsSession = ((IAudioSessionControl2)_session).IsSystemSoundsSession() == 0;
            ProcessId = ReadProcessId();
            _parent = new WeakReference<IAudioDevice>(parent);

            _appInfo = AppInformationService.GetInformationForAppByPid(ProcessId);

            if (string.IsNullOrWhiteSpace(_appInfo.SmallLogoPath))
            {
                _appInfo.SmallLogoPath = session.GetIconPath();
            }

            // NOTE: Ensure that the callbacks won't touch state that isn't initialized yet (i.e. _appInfo must be valid before the first callback)
            _session.RegisterAudioSessionNotification(this);
            _isRegistered = true;
            ((IAudioSessionControl2)_session).GetSessionInstanceIdentifier(out _id);

            Trace.WriteLine($"AudioDeviceSession Create {ExeName} {_id}");

            if (_appInfo.CanTrack)
            {
                ProcessWatcherService.WatchProcess(ProcessId, (pid) => DisconnectSession());
            }

            ReadRawDisplayName();
            RefreshDisplayName();

            if (parent.Parent.DeviceKind == AudioDeviceKind.Recording)
            {
                _isDisconnected = IsSystemSoundsSession;
            }
            else
            {
                SyncPersistedEndpoint(parent);
            }
        }

        ~AudioDeviceSession()
        {
            try
            {
                if (_isRegistered)
                {
                    _session.UnregisterAudioSessionNotification(this);
                }
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
                _refreshDisplayNameTask = Task.Delay(TimeSpan.FromSeconds(5));
                var internalRefreshDisplayNameTask = new Task(() =>
                {
                    var displayName = AppInformationService.GetDisplayNameForAppByPid(ProcessId);
                    _dispatcher.BeginInvoke((Action)(() =>
                    {
                        _resolvedAppDisplayName = displayName;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SessionDisplayName)));
                    }));
                });
                internalRefreshDisplayNameTask.ContinueWith((inTask) => _refreshDisplayNameTask);
                internalRefreshDisplayNameTask.Start();
            }
        }

        public void Hide()
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

        public void UnHide()
        {
            Trace.WriteLine($"AudioDeviceSession UnHide {ExeName} {Id}");

            _isMoved = false;
            _moveOnInactive = false;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(State)));
        }

        public void MoveToDevice(string id, bool hide)
        {
            if (_parent.TryGetTarget(out var parent))
            {
                parent.Parent.SetDefaultEndPoint(id, ProcessId);
            }
        }

        public void UpdatePeakValueBackground()
        {
            var newValues = Helpers.ReadPeakValues(_meter);
            PeakValue1 = newValues[0];
            PeakValue2 = newValues[1];
        }

        private int ReadProcessId()
        {
            var hr = ((IAudioSessionControl2)_session).GetProcessId(out uint pid);

            if (hr == (int)Error.AUDCLNT_S_NO_SINGLE_PROCESS ||
                hr == (int)Error.S_OK)
            {
                // NOTE: This is a workaround for what seems to be a Windows 10 OS bug.
                // Sometimes the session (which is IsSystemSoundsSession) has a nonzero PID.
                // The PID refers to taskhostw.exe.  In these cases the session is correctly
                // wired up and does reflect the system sounds.  If we restart the app the system
                // will give us a session which has IsSystemSounds=true/pid=0, so this seems to be a bug
                // in letting system sounds played through taskhostw.exe bleed through.
                return IsSystemSoundsSession ? 0 : (int)pid;
            }

            throw new COMException($"Failed: 0x{hr.ToString("X")}", hr);
        }

        private void SyncPersistedEndpoint(IAudioDevice parent)
        {
            // NOTE: This is to work around what we believe to be a Windows 10 OS bug!
            // The audio session redirection feature became available in 1803 and this bug appears
            // to impact some or all UWP applications.
            //
            // Repro:
            // - Using a UWP app like Minesweeper
            // - Open the app on default device A and play a sound (observe sound plays on A)
            // - Move the app to device B
            // - Play a sound (observe the sound plays on B)
            // - Close app
            // - Open app again and play a sound
            // EXPECTED: App plays on device B since it is configured and available
            // ACTUAL: App plays on device A which is incorrect.
            //
            // We work around this by attempting to move the session to the specified persisted endpoint.
            // If we fail for any reason (including that device no longer being available at all), we expect
            // to continue without issue using the current parent device.
            var persistedDeviceId = PersistedDefaultEndPointId;
            if (!string.IsNullOrWhiteSpace(persistedDeviceId))
            {
                if (parent.Id != persistedDeviceId)
                {
                    Trace.WriteLine($"FORCE-MOVE: {_state} {parent.Id} -> {persistedDeviceId}");

                    MoveToDevice(persistedDeviceId, false);

                    // There is an inherence race condition in the system when calling the API to move the session to
                    // another endpoint.  The system will very quickly make the session Active and then Inactive.  This
                    // event causes us to lose the _isMoved/_moveOnInactive state, which means the session becomes visible again.
                    // In order to work around this, we add a short delay so the system will have issued the events and either the
                    // session is still playing (in which case the user will see it) or it is back to Inactive and we can hide it.
                    var delay = Task.Delay(TimeSpan.FromMilliseconds(200));
                    delay.ContinueWith((_) =>
                    {
                        _dispatcher.BeginInvoke((Action)(() =>
                        {
                            if (_state == AudioSessionState.Active)
                            {
                                _moveOnInactive = true;
                            }
                            else
                            {
                                _isMoved = true;
                            }

                            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(State)));
                        }));
                    });
                }
            }
        }

        private void ReadRawDisplayName()
        {
            try
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
            catch (Exception ex) when (ex.Is(Error.AUDCLNT_E_DEVICE_INVALIDATED))
            {
                // Expected in some cases.
            }
        }

        private void DisconnectSession()
        {
            Trace.WriteLine($"AudioDeviceSession DisconnectSession {ExeName} {Id}");

            _isDisconnected = true;
            _dispatcher.BeginInvoke((Action)(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(State)));
            }));
        }

        void IAudioSessionEvents.OnSimpleVolumeChanged(float NewVolume, int NewMute, ref Guid EventContext)
        {
            _volume = NewVolume;
            _isMuted = NewMute != 0;

            _dispatcher.BeginInvoke((Action)(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Volume)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsMuted)));
            }));
        }

        void IAudioSessionEvents.OnGroupingParamChanged(ref Guid NewGroupingParam, ref Guid EventContext)
        {
            GroupingParam = NewGroupingParam;
            Trace.WriteLine($"AudioDeviceSession OnGroupingParamChanged {ExeName} {Id}");
            _dispatcher.BeginInvoke((Action)(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GroupingParam)));
            }));
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

            _dispatcher.BeginInvoke((Action)(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(State)));
            }));
        }

        void IAudioSessionEvents.OnDisplayNameChanged(string NewDisplayName, ref Guid EventContext)
        {
            ReadRawDisplayName();

            _dispatcher.BeginInvoke((Action)(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SessionDisplayName)));
            }));
        }

        void IAudioSessionEvents.OnSessionDisconnected(AudioSessionDisconnectReason DisconnectReason) => DisconnectSession();

        void IAudioSessionEvents.OnChannelVolumeChanged(uint ChannelCount, ref float NewChannelVolumeArray, uint ChangedChannel, ref Guid EventContext) { }
        void IAudioSessionEvents.OnIconPathChanged(string NewIconPath, ref Guid EventContext) { }
    }
}
