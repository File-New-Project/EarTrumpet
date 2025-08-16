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
using System.Windows;
using System.Windows.Threading;
using Windows.Win32;
using Windows.Win32.Media.Audio;
using Windows.Win32.Media.Audio.Endpoints;

namespace EarTrumpet.DataModel.WindowsAudio.Internal;

internal class AudioDeviceSession : BindableBase, IAudioSessionEvents, IAudioDeviceSession, IAudioDeviceSessionInternal
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
        get => App.Settings.UseLogarithmicVolume
            ? _volume.LinearToLog()
            : _volume;
        set
        {
            try
            {
                if (App.Settings.UseLogarithmicVolume)
                {
                    value = value.Bound(App.Settings.LogarithmicVolumeMinDb, 0);
                    // We must convert manually here because sessions use linear volume.
                    _simpleVolume.SetMasterVolume(value.LogToLinear(), Guid.Empty);
                    _volume = value;
                    IsMuted = value <= App.Settings.LogarithmicVolumeMinDb;
                }
                else
                {
                    value = value.Bound(0, 1f);
                    _simpleVolume.SetMasterVolume(value, Guid.Empty);
                    _volume = value;
                    IsMuted = _volume.ToVolumeInt() == 0;
                }
            }
            catch (Exception ex) when (ex.Is(HRESULT.AUDCLNT_E_DEVICE_INVALIDATED))
            {
                // Expected in some cases.
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
                    _simpleVolume.SetMute(value ? new BOOL(1) : new BOOL(0), Guid.Empty);
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
        public string IconPath { get; private set; }
        public Guid GroupingParam { get; private set; }
        public float PeakValue1 { get; private set; }
        public float PeakValue2 { get; private set; }
        public bool IsDesktopApp => _appInfo.IsDesktopApp;
        public string AppId => _appInfo.AppId;

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
                case AudioSessionState.AudioSessionStateActive:
                    return SessionState.Active;
                case AudioSessionState.AudioSessionStateInactive:
                    return SessionState.Inactive;
                case AudioSessionState.AudioSessionStateExpired:
                    return SessionState.Expired;
                default:
                    throw new NotImplementedException();
            }
        }
    }

    public uint ProcessId { get; }

    public string Id => _id;

    public bool IsSystemSoundsSession { get; }

        public ObservableCollection<IAudioDeviceSession> Children { get; private set; }
        public IEnumerable<IAudioDeviceSessionChannel> Channels => _channels.Channels;
        public string PackageInstallPath { get; private set; }

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

        _session.GetState(out _state);
        _session.GetGroupingParam(out var groupingParam);
        GroupingParam = groupingParam;

        _simpleVolume.GetMasterVolume(out _volume);
        
        _simpleVolume.GetMute(out var isMuted);
        _isMuted = isMuted;

        IsSystemSoundsSession = ((IAudioSessionControl2)_session).IsSystemSoundsSession() == HRESULT.S_OK;
        ProcessId = ReadProcessId();
        _parent = new WeakReference<IAudioDevice>(parent);

            _appInfo = AppInformationFactory.CreateForProcess(ProcessId, trackProcess: true);
            _appInfo.Stopped += _ => DisconnectSession();
            PackageInstallPath = _appInfo.PackageInstallPath;

        // NOTE: Ensure that the callbacks won't touch state that isn't initialized yet (i.e. _appInfo must be valid before the first callback)
        _session.RegisterAudioSessionNotification(this);
        _isRegistered = true;
        ((IAudioSessionControl2)_session).GetSessionInstanceIdentifier(out var sessionId);
        _id = sessionId.ToString();

        Trace.WriteLine($"AudioDeviceSession Create {ExeName} {_id}");

        session.GetIconPath(out var iconPath);
        ChooseIconPath(iconPath.ToString());
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

        WeakEventManager<AppSettings, EventArgs>.AddHandler(
            App.Settings,
            nameof(AppSettings.UseLogarithmicVolumeChanged),
            UseLogarithmicVolumeChangedHandler);
    }

    private void UseLogarithmicVolumeChangedHandler(object sender, EventArgs e)
    {
        RaisePropertyChanged(nameof(Volume));
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
            Trace.WriteLine($"AudioDeviceSession dtor Failed: {ex}");
        }
    }

    public void Hide()
    {
        Trace.WriteLine($"AudioDeviceSession MoveFromDevice {ExeName} {Id}");

        if (_state == AudioSessionState.AudioSessionStateActive)
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
        if (App.Settings.UseLogarithmicVolume)
        {
            PeakValue1 = newValues[0].LinearToLogNormalized();
            PeakValue2 = newValues[1].LinearToLogNormalized();
        }
        else
        {
            PeakValue1 = newValues[0];
            PeakValue2 = newValues[1];
        }
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

    private void ChooseIconPath(string iconPathFromSession)
    {
        if (!string.IsNullOrWhiteSpace(iconPathFromSession) && !IsSystemSoundsSession)
        {
            IconPath = iconPathFromSession;
        }
        else if (!string.IsNullOrWhiteSpace(_appInfo.SmallLogoPath))
        {
            IconPath = _appInfo.SmallLogoPath;
        }
        else
        {
            IconPath = null;
        }
    }

    private uint ReadProcessId()
    {
        var hr = ((IAudioSessionControl2)_session).GetProcessId(out var pid);

        if (hr == (int)HRESULT.AUDCLNT_S_NO_SINGLE_PROCESS ||
            hr == (int)HRESULT.S_OK)
        {
            // NOTE: This is a workaround for what seems to be a Windows 10 OS bug.
            // Sometimes the session (which is IsSystemSoundsSession) has a nonzero PID.
            // The PID refers to taskhostw.exe.  In these cases the session is correctly
            // wired up and does reflect the system sounds.  If we restart the app the system
            // will give us a session which has IsSystemSounds=true/pid=0, so this seems to be a bug
            // in letting system sounds played through taskhostw.exe bleed through.
            return IsSystemSoundsSession ? 0 : pid;
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
                        if (_state == AudioSessionState.AudioSessionStateActive)
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
            _session.GetDisplayName(out var pDisplayName);
            var displayName = pDisplayName.ToString();
            if (displayName.StartsWith("@", StringComparison.Ordinal))
            {
                Span<char> buffer = stackalloc char[512];
                unsafe
                {
                    fixed(char* bufferPtr = buffer)
                    {
                        if (PInvoke.SHLoadIndirectString(displayName, bufferPtr, (uint)buffer.Length) == 0)
                        {
                            displayName = new PWSTR(bufferPtr).ToString();
                        }
                    }
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

    unsafe void IAudioSessionEvents.OnSimpleVolumeChanged(float NewVolume, BOOL NewMute, Guid* EventContext)
    {
        _volume = NewVolume;
        _isMuted = NewMute != 0;

        _dispatcher.BeginInvoke((Action)(() =>
        {
            RaisePropertyChanged(nameof(Volume));
            RaisePropertyChanged(nameof(IsMuted));
        }));
    }

    unsafe void IAudioSessionEvents.OnGroupingParamChanged(Guid* NewGroupingParam, Guid* EventContext)
    {
        GroupingParam = *NewGroupingParam;
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

        if (_isMoved && NewState == AudioSessionState.AudioSessionStateActive)
        {
            _isMoved = false;
        }
        else if (_moveOnInactive && NewState == AudioSessionState.AudioSessionStateInactive)
        {
            _isMoved = true;
            _moveOnInactive = false;
        }

        _dispatcher.BeginInvoke((Action)(() =>
        {
            RaisePropertyChanged(nameof(State));
        }));
    }

    unsafe void IAudioSessionEvents.OnDisplayNameChanged(PCWSTR NewDisplayName, Guid* EventContext)
    {
        ChooseDisplayName(NewDisplayName.ToString());

        _dispatcher.BeginInvoke((Action)(() =>
        {
            RaisePropertyChanged(nameof(DisplayName));
        }));
    }

    void IAudioSessionEvents.OnSessionDisconnected(AudioSessionDisconnectReason DisconnectReason) => DisconnectSession();

    unsafe void IAudioSessionEvents.OnChannelVolumeChanged(uint ChannelCount, float[] afNewChannelVolume, uint ChangedChannel, Guid* EventContext)
    {
        var channelVolumesValues = new float[ChannelCount];
        Array.Copy(afNewChannelVolume, 0, channelVolumesValues, 0, (int)ChannelCount);

        for (var i = 0; i < ChannelCount; i++)
        {
            _channels.Channels[i].SetLevel(channelVolumesValues[i]);
        }
    }

    unsafe void IAudioSessionEvents.OnIconPathChanged(PCWSTR NewIconPath, Guid* EventContext)
    {
        IconPath = NewIconPath.ToString();
        RaisePropertyChanged(nameof(IconPath));
    }
}
