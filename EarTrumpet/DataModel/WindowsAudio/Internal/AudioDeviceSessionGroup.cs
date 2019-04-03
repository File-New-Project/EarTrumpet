using EarTrumpet.DataModel.Audio;
using EarTrumpet.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace EarTrumpet.DataModel.WindowsAudio.Internal
{
    class AudioDeviceSessionGroup : BindableBase, IAudioDeviceSession, IAudioDeviceSessionInternal
    {
        public IAudioDevice Parent => _sessions.Count > 0 ? _sessions[0].Parent : null;

        public IEnumerable<IAudioDeviceSessionChannel> Channels
        {
            get
            {
                var buckets = new List<List<IAudioDeviceSessionChannel>>();
                foreach(var session in _sessions)
                {
                    var sessionChannels = session.Channels.ToList();
                    for (var i = 0; i < sessionChannels.Count; i++)
                    {
                        if (buckets.Count <= i)
                        {
                            buckets.Add(new List<IAudioDeviceSessionChannel>());
                        }

                        buckets[i].Add(sessionChannels[i]);
                    }
                }

                var ret = new List<IAudioDeviceSessionChannel>();
                foreach(var bucket in buckets)
                {
                    ret.Add(new AudioDeviceSessionChannelMultiplexer(bucket.ToArray()));
                }

                return ret;
            }
        }
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

        public bool IsSystemSoundsSession => _sessions.Any(s => s.IsSystemSoundsSession);

        public float PeakValue1 => _sessions.Count > 0 ? _sessions.Max(s => s.PeakValue1) : 0;
        public float PeakValue2 => _sessions.Count > 0 ? _sessions.Max(s => s.PeakValue2) : 0;

        public int ProcessId => _sessions.Count > 0 ? _sessions[0].ProcessId : -1;

        public SessionState State
        {
            get
            {
                if (_sessions.FirstOrDefault(s => s.State == SessionState.Active) != null)
                {
                    return SessionState.Active;
                }
                if (_sessions.FirstOrDefault(s => s.State == SessionState.Inactive) != null)
                {
                    return SessionState.Inactive;
                }
                if (_sessions.FirstOrDefault(s => s.State == SessionState.Moved) != null)
                {
                    return SessionState.Moved;
                }
                return SessionState.Expired;
            }
        }

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

        public ObservableCollection<IAudioDeviceSession> Children => _sessions;

        public void Hide()
        {
            foreach (var session in _sessions.ToArray())
            {
                ((IAudioDeviceSessionInternal)session).Hide();
            }
        }

        public void UnHide()
        {
            foreach (var session in _sessions.ToArray())
            {
                ((IAudioDeviceSessionInternal)session).UnHide();
            }
        }

        public void MoveToDevice(string id, bool hideExistingSessions)
        {
            if (_parent.TryGetTarget(out var parent))
            {
                // Update the output for all processes represented by this app.
                foreach (var pid in _sessions.Select(c => c.ProcessId).ToSet())
                {
                    ((IAudioDeviceManagerWindowsAudio)parent.Parent).SetDefaultEndPoint(id, pid);
                }

                if (hideExistingSessions)
                {
                    Hide();
                }
            }
        }

        public void UpdatePeakValueBackground()
        {
            // We're in the background so we need to use a snapshot.
            foreach (var session in _sessions.ToArray())
            {
                ((IAudioDeviceSessionInternal)session).UpdatePeakValueBackground();
            }
        }

        private readonly ObservableCollection<IAudioDeviceSession> _sessions = new ObservableCollection<IAudioDeviceSession>();
        private string _id;
        private readonly WeakReference<IAudioDevice> _parent;

        public AudioDeviceSessionGroup(IAudioDevice parent, IAudioDeviceSession session)
        {
            _parent = new WeakReference<IAudioDevice>(parent);
            GroupingParam = ((IAudioDeviceSessionInternal)session).GroupingParam; // can change at runtime
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
            RaisePropertyChanged(e.PropertyName);
        }
    }
}
