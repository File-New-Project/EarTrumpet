using EarTrumpet.DataModel.Internal.Services;
using EarTrumpet.Extensions;
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

        public uint BackgroundColor => _sessions.Count > 0 ? _sessions[0].BackgroundColor : 0;

        public string DisplayName => _sessions.Count > 0 ? _sessions[0].DisplayName : null;

        public string ExeName => _sessions.Count > 0 ? _sessions[0].ExeName : null;

        public Guid GroupingParam { get; private set; }

        public string IconPath => _sessions.Count > 0 ? _sessions[0].IconPath : null;

        public string Id => _id;

        public bool IsDesktopApp => _sessions.Count > 0 ? _sessions[0].IsDesktopApp : false;

        public string AppId { get; private set; }

        public bool IsMuted
        {
            get => _sessions.Count > 0 ? _sessions[0].IsMuted : false;
            set
            {
                foreach (var session in _sessions)
                {
                    session.IsMuted = value;
                }
            }
        }

        public bool IsSystemSoundsSession => _sessions.Count > 0 ? _sessions[0].IsSystemSoundsSession : false;

        public float PeakValue => _sessions.Count > 0 ? _sessions.Max(s => s.PeakValue) : 0;

        public int ProcessId => _sessions.Count > 0 ? _sessions[0].ProcessId : -1;

        public SessionState State => _sessions.Count > 0 ? _sessions[0].State : SessionState.Invalid;

        public float Volume
        {
            get => _sessions.Count > 0 ? _sessions[0].Volume : 1;
            set
            {
                foreach (var session in _sessions)
                {
                    session.Volume = value;
                }
            }
        }

        public string PersistedDefaultEndPointId => _sessions.Count > 0 ? _sessions[0].PersistedDefaultEndPointId : null;

        public ObservableCollection<IAudioDeviceSession> Children => _sessions;

        public void MoveFromDevice()
        {
            foreach (var session in _sessions.ToArray())
            {
                session.MoveFromDevice();
            }
        }

        public void MoveAllSessionsToDevice(string id)
        {
            // Update the output for all processes represented by this app.
            foreach (var pid in _sessions.Select(c => c.ProcessId).ToSet())
            {
                AudioPolicyConfigService.SetDefaultEndPoint(id, pid);
            }

            MoveFromDevice();
        }

        private ObservableCollection<IAudioDeviceSession> _sessions = new ObservableCollection<IAudioDeviceSession>();
        private string _id;

        public AudioDeviceSessionGroup(IAudioDeviceSession session)
        {
            GroupingParam = session.GroupingParam; // can change at runtime
            AppId = session.AppId;

            AddSession(session);
        }

        ~AudioDeviceSessionGroup()
        {
            foreach (var session in _sessions)
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

        public void RefreshDisplayName()
        {
            foreach (var session in Sessions)
            {
                session.RefreshDisplayName();
            }
        }
    }
}
