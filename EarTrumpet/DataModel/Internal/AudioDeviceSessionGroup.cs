using EarTrumpet.Interop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace EarTrumpet.DataModel.Internal
{
    // Session multiplexing container (for grouping)
    public class AudioDeviceSessionGroup : IAudioDeviceSession
    {
        ObservableCollection<IAudioDeviceSession> _sessions = new ObservableCollection<IAudioDeviceSession>();
        string _id;

        public AudioDeviceSessionGroup(IAudioDeviceSession session)
        {
            // GroupingParam can change at runtime, so we can't trust session[0].
            GroupingParam = session.GroupingParam;

            AddSession(session);
        }

        ~AudioDeviceSessionGroup()
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

            if (_id == null)
            {
                _id = _sessions[0].Id;
            }
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

        public string Id => _id;

        public bool IsDesktopApp => _sessions[0].IsDesktopApp;

        public string AppId => _sessions[0].AppId;

        public bool IsMuted {
            get => _sessions[0].IsMuted;
            set
            {
                foreach (var session in _sessions)
                {
                    session.IsMuted = value;
                }
            }
        }

        public bool IsSystemSoundsSession => _sessions[0].IsSystemSoundsSession;

        public float PeakValue => _sessions[0].PeakValue;

        public int ProcessId => _sessions[0].ProcessId;

        public AudioSessionState State => _sessions[0].State;

        public float Volume
        {
            get => _sessions[0].Volume;
            set
            {
                foreach (var session in _sessions)
                {
                    session.Volume = value;
                }
            }
        }

        public ObservableCollection<IAudioDeviceSession> Children => _sessions;

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
