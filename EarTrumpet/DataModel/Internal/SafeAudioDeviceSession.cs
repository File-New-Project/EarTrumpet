using EarTrumpet.Interop;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace EarTrumpet.DataModel.Internal
{
    // Avoid device invalidation COMExceptions from bubbling up out of devices that have been removed.
    class SafeAudioDeviceSession : IAudioDeviceSession
    {
        IAudioDeviceSession _session;

        public SafeAudioDeviceSession(IAudioDeviceSession session)
        {
            _session = session;
            _session.PropertyChanged += Session_PropertyChanged;
        }

        ~SafeAudioDeviceSession()
        {
            _session.PropertyChanged -= Session_PropertyChanged;
        }

        private void Session_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        public IAudioDevice Device => _session.Device;

        public uint BackgroundColor => SafeCallHelper.GetValue(() => _session.BackgroundColor);

        public Guid GroupingParam => SafeCallHelper.GetValue(() => _session.GroupingParam);

        public string IconPath => SafeCallHelper.GetValue(() => _session.IconPath);

        public bool IsDesktopApp => SafeCallHelper.GetValue(() => _session.IsDesktopApp);

        public string AppId => SafeCallHelper.GetValue(() => _session.AppId);

        public bool IsSystemSoundsSession => SafeCallHelper.GetValue(() => _session.IsSystemSoundsSession);

        public int ProcessId => SafeCallHelper.GetValue(() => _session.ProcessId);

        public AudioSessionState State => SafeCallHelper.GetValue(() => _session.State);

        public string DisplayName => SafeCallHelper.GetValue(() => _session.DisplayName);
        public string RawDisplayName => SafeCallHelper.GetValue(() => ((AudioDeviceSession)_session).RawDisplayName);

        public string Id => SafeCallHelper.GetValue(() => _session.Id);

        public bool IsMuted { get => SafeCallHelper.GetValue(() => _session.IsMuted); set => SafeCallHelper.SetValue(() => _session.IsMuted = value); }
        public float Volume { get => SafeCallHelper.GetValue(() => _session.Volume); set => SafeCallHelper.SetValue(() => _session.Volume = value); }

        public float PeakValue => SafeCallHelper.GetValue(() => _session.PeakValue);

        public ObservableCollection<IAudioDeviceSession> Children => SafeCallHelper.GetValue(() => _session.Children);

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
