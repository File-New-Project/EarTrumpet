using EarTrumpet.DataModel.Com;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace EarTrumpet.DataModel
{
    // Session multiplexing container (for grouping)
    public class AudioDeviceSessionContainer : IAudioDeviceSession
    {
        List<IAudioDeviceSession> _sessions = new List<IAudioDeviceSession>();

        public AudioDeviceSessionContainer(IAudioDeviceSession session)
        {
            // GroupingParam can change at runtime, so we can't trust session[0].
            GroupingParam = session.GroupingParam;

            AddSession(session);
        }

        ~AudioDeviceSessionContainer()
        {
            foreach(var session in _sessions)
            {
                session.PropertyChanged -= Session_PropertyChanged;
            }
        }

        private void Session_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        public IEnumerable<IAudioDeviceSession> Sessions => _sessions;

        public void AddSession(IAudioDeviceSession session)
        {
            _sessions.Add(session);

            session.PropertyChanged += Session_PropertyChanged;

            // Inherit properties (safely) from existing streams
            session.IsMuted = _sessions[0].IsMuted || session.IsMuted;
        }

        public void RemoveSession(IAudioDeviceSession session)
        {
            session.PropertyChanged -= Session_PropertyChanged;
            _sessions.Remove(session);
        }

        public IAudioDevice Device => _sessions[0].Device;

        public uint BackgroundColor => _sessions[0].BackgroundColor;

        public string DisplayName => _sessions[0].DisplayName;

        public Guid GroupingParam { get; private set; }

        public string IconPath => _sessions[0].IconPath;

        public string Id => _sessions[0].Id;

        public bool IsDesktopApp => _sessions[0].IsDesktopApp;

        public bool IsMuted { get => _sessions[0].IsMuted; set => _sessions.ForEach(s => s.IsMuted = value); }

        public bool IsSystemSoundsSession => _sessions[0].IsSystemSoundsSession;

        public float PeakValue => _sessions[0].PeakValue;

        public int ProcessId => _sessions[0].ProcessId;

        public AudioSessionState State => _sessions[0].State;

        public string ActiveOnOtherDevice
        {
            get => _sessions[0].ActiveOnOtherDevice;
            set => _sessions[0].ActiveOnOtherDevice = value;
        }

        public float Volume { get => _sessions[0].Volume; set => _sessions.ForEach(s => s.Volume = value); }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
