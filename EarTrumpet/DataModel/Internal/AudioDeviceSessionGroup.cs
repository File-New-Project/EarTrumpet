using EarTrumpet.Interop.MMDeviceAPI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace EarTrumpet.DataModel.Internal
{
    class AudioDeviceSessionGroup : IAudioDeviceSession
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public IEnumerable<IAudioDeviceSession> Sessions => _sessions;

        public IAudioDevice Device => _sessions[0].Device;

        public uint BackgroundColor => _sessions[0].BackgroundColor;

        public string DisplayName => _sessions[0].DisplayName;

        public string ExeName => _sessions[0].ExeName;

        public Guid GroupingParam { get; private set; }

        public string IconPath => _sessions[0].IconPath;

        public string Id => _id;

        public bool IsDesktopApp => _sessions[0].IsDesktopApp;

        public string AppId => _sessions[0].AppId;

        public bool IsMuted
        {
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

        public float PeakValue => _sessions.Max(s => s.PeakValue);

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

        private ObservableCollection<IAudioDeviceSession> _sessions = new ObservableCollection<IAudioDeviceSession>();
        private string _id;

        public AudioDeviceSessionGroup(IAudioDeviceSession session)
        {
            GroupingParam = session.GroupingParam; // can change at runtime

            AddSession(session);
        }

        ~AudioDeviceSessionGroup()
        {
            foreach(var session in _sessions)
            {
                session.PropertyChanged -= Session_PropertyChanged;
            }
        }

        public void AddSession(IAudioDeviceSession session)
        {
            if (_id == null)
            {
                _id = session.Id;
            }

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

        private void Session_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }
    }
}
