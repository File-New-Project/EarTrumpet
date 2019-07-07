using EarTrumpet.DataModel.AppInformation;
using EarTrumpet.DataModel.Audio;
using EarTrumpet.Extensions;
using EarTrumpet.Interop;
using EarTrumpet.Interop.MMDeviceAPI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace EarTrumpet.DataModel.WindowsAudio.Internal
{
    class AudioDeviceSession : BindableBase, IAudioSessionEvents, IAudioDeviceSession, IAudioDeviceSessionInternal
    {
        public IAudioDevice Parent
        {
            get
            {
                if (_parent.TryGetTarget(out var parent))
                {
                    return parent;
                }
                return null;
            }
        }

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
                    catch (Exception ex) when (ex.Is(HRESULT.AUDCLNT_E_DEVICE_INVALIDATED))
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
                    catch (Exception ex) when (ex.Is(HRESULT.AUDCLNT_E_DEVICE_INVALIDATED))
                    {
                        // Expected in some cases.
                    }
                }
            }
        }

        public string DisplayName { get; private set; }
        public string ExeName => _appInfo.ExeName;
        public string IconPath { get; }
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

        public ObservableCollection<IAudioDeviceSession> Children { get; private set; }
        public IEnumerable<IAudioDeviceSessionChannel> Channels => _channels.Channels;

        private readonly string _id;
        private readonly IAudioSessionControl _session;
        private readonly ISimpleAudioVolume _simpleVolume;
        private readonly IAudioMeterInformation _meter;
        private readonly Dispatcher _dispatcher;
        private readonly IAppInfo _appInfo;
        private readonly AudioDeviceSessionChannelCollection _channels;
        private float _volume;
        private AudioSessionState _state;
        private bool _isMuted;
        private bool _isDisconnected;
        private bool _isMoved;
        private bool _moveOnInactive;
        private bool _isRegistered;
        private WeakReference<IAudioDevice> _parent;

        public AudioDeviceSession(IAudioDevice parent, IAudioSessionControl session, Dispatcher foregroundDispatcher)
        {
            _dispatcher = foregroundDispatcher;
            _session = session;
            _meter = (IAudioMeterInformation)_session;
            _simpleVolume = (ISimpleAudioVolume)session;
            _channels = new AudioDeviceSessionChannelCollection((IChannelAudioVolume)session, _dispatcher);
            _state = _session.GetState();
            GroupingParam = _session.GetGroupingParam();
            _simpleVolume.GetMasterVolume(out _volume);
            _isMuted = _simpleVolume.GetMute() != 0;
            IsSystemSoundsSession = ((IAudioSessionControl2)_session).IsSystemSoundsSession() == HRESULT.S_OK;
            ProcessId = ReadProcessId();
            _parent = new WeakReference<IAudioDevice>(parent);

            _appInfo = AppInformationFactory.CreateForProcess(ProcessId, trackProcess: true);
            _appInfo.Stopped += _ => DisconnectSession();

            IconPath = string.IsNullOrWhiteSpace(_appInfo.SmallLogoPath) ? session.GetIconPath() : _appInfo.SmallLogoPath;

            // NOTE: Ensure that the callbacks won't touch state that isn't initialized yet (i.e. _appInfo must be valid before the first callback)
            _session.RegisterAudioSessionNotification(this);
            _isRegistered = true;
            ((IAudioSessionControl2)_session).GetSessionInstanceIdentifier(out _id);

            Trace.WriteLine($"AudioDeviceSession Create {ExeName} {_id}");

            ChooseDisplayName(ReadSessionDisplayName());

            if (parent.Parent != null)
            {
                if (parent.Parent.Kind == AudioDeviceKind.Recording.ToString())
                {
                    _isDisconnected = IsSystemSoundsSession;
                }
                else
                {
                    SyncPersistedEndpoint(parent);
                }
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
                Trace.WriteLine($"{ex}");
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
                RaisePropertyChanged(nameof(State));
            }
        }

        public void UnHide()
        {
            Trace.WriteLine($"AudioDeviceSession UnHide {ExeName} {Id}");

            _isMoved = false;
            _moveOnInactive = false;

            RaisePropertyChanged(nameof(State));
        }

        public void MoveToDevice(string id, bool hide)
        {
            if (_parent.TryGetTarget(out var parent))
            {
                ((IAudioDeviceManagerWindowsAudio)parent.Parent).SetDefaultEndPoint(id, ProcessId);
            }
        }

        public void UpdatePeakValueBackground()
        {
            var newValues = Helpers.ReadPeakValues(_meter);
            PeakValue1 = newValues[0];
            PeakValue2 = newValues[1];
        }

        private void ChooseDisplayName(string displayNameFromSession)
        {
            if (!string.IsNullOrWhiteSpace(displayNameFromSession))
            {
                DisplayName = displayNameFromSession;
            }
            else if (!string.IsNullOrWhiteSpace(_appInfo.DisplayName))
            {
                DisplayName = _appInfo.DisplayName;
            }
            else
            {
                DisplayName = _appInfo.ExeName;
            }
        }

        private int ReadProcessId()
        {
            var hr = ((IAudioSessionControl2)_session).GetProcessId(out uint pid);

            if (hr == (int)HRESULT.AUDCLNT_S_NO_SINGLE_PROCESS ||
                hr == (int)HRESULT.S_OK)
            {
                // NOTE: This is a workaround for what seems to be a Windows 10 OS bug.
                // Sometimes the session (which is IsSystemSoundsSession) has a nonzero PID.
                // The PID refers to taskhostw.exe.  In these cases the session is correctly
                // wired up and does reflect the system sounds.  If we restart the app the system
                // will give us a session which has IsSystemSounds=true/pid=0, so this seems to be a bug
                // in letting system sounds played through taskhostw.exe bleed through.
                return IsSystemSoundsSession ? 0 : (int)pid;
            }

            throw Marshal.GetExceptionForHR(hr);
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
            var persistedDeviceId = ((IAudioDeviceManagerWindowsAudio)Parent.Parent).GetDefaultEndPoint(ProcessId);
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

                            RaisePropertyChanged(nameof(State));
                        }));
                    });
                }
            }
        }

        private string ReadSessionDisplayName()
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

                return displayName;
            }
            catch (Exception ex) when (ex.Is(HRESULT.AUDCLNT_E_DEVICE_INVALIDATED))
            {
                // Expected in some cases.
            }
            return null;
        }

        private void DisconnectSession()
        {
            Trace.WriteLine($"AudioDeviceSession DisconnectSession {ExeName} {Id}");

            _isDisconnected = true;
            _dispatcher.BeginInvoke((Action)(() =>
            {
                RaisePropertyChanged(nameof(State));
            }));
        }

        void IAudioSessionEvents.OnSimpleVolumeChanged(float NewVolume, int NewMute, ref Guid EventContext)
        {
            _volume = NewVolume;
            _isMuted = NewMute != 0;

            _dispatcher.BeginInvoke((Action)(() =>
            {
                RaisePropertyChanged(nameof(Volume));
                RaisePropertyChanged(nameof(IsMuted));
            }));
        }

        void IAudioSessionEvents.OnGroupingParamChanged(ref Guid NewGroupingParam, ref Guid EventContext)
        {
            GroupingParam = NewGroupingParam;
            Trace.WriteLine($"AudioDeviceSession OnGroupingParamChanged {ExeName} {Id}");
            _dispatcher.BeginInvoke((Action)(() =>
            {
                RaisePropertyChanged(nameof(GroupingParam));
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
                RaisePropertyChanged(nameof(State));
            }));
        }

        void IAudioSessionEvents.OnDisplayNameChanged(string NewDisplayName, ref Guid EventContext)
        {
            ChooseDisplayName(NewDisplayName);

            _dispatcher.BeginInvoke((Action)(() =>
            {
                RaisePropertyChanged(nameof(DisplayName));
            }));
        }

        void IAudioSessionEvents.OnSessionDisconnected(AudioSessionDisconnectReason DisconnectReason) => DisconnectSession();

        void IAudioSessionEvents.OnChannelVolumeChanged(uint ChannelCount, IntPtr afNewChannelVolume, uint ChangedChannel, ref Guid EventContext)
        {
            var channelVolumesValues = new float[ChannelCount];
            Marshal.Copy(afNewChannelVolume, channelVolumesValues, 0, (int)ChannelCount);

            for (var i = 0; i < ChannelCount; i++)
            {
                _channels.Channels[i].SetLevel(channelVolumesValues[i]);
            }
        }
        void IAudioSessionEvents.OnIconPathChanged(string NewIconPath, ref Guid EventContext) { }
    }
}
