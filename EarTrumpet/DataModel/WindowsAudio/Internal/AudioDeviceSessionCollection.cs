using EarTrumpet.DataModel.Audio;
using EarTrumpet.Interop.MMDeviceAPI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Threading;

namespace EarTrumpet.DataModel.WindowsAudio.Internal
{
    class AudioDeviceSessionCollection : IAudioSessionNotification
    {
        public ObservableCollection<IAudioDeviceSession> Sessions => _sessions;

        private readonly Dispatcher _dispatcher;
        private readonly ObservableCollection<IAudioDeviceSession> _sessions = new ObservableCollection<IAudioDeviceSession>();
        private readonly List<IAudioDeviceSession> _movedSessions = new List<IAudioDeviceSession>();
        private IAudioSessionManager2 _sessionManager;
        private WeakReference<IAudioDevice> _parent;

        public AudioDeviceSessionCollection(IAudioDevice parent, IMMDevice device, Dispatcher foregroundDispatcher)
        {
            _parent = new WeakReference<IAudioDevice>(parent);
            _dispatcher = foregroundDispatcher;

            try
            {
                _sessionManager = device.Activate<IAudioSessionManager2>();
                _sessionManager.RegisterSessionNotification(this);
                var enumerator = _sessionManager.GetSessionEnumerator();
                int count = enumerator.GetCount();
                for (int i = 0; i < count; i++)
                {
                    CreateAndAddSession(enumerator.GetSession(i));
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"AudioDeviceSessionCollection Create dev={device.GetId()} {ex}");
            }
        }

        ~AudioDeviceSessionCollection()
        {
            foreach (var session in _sessions)
            {
                session.PropertyChanged -= Session_PropertyChanged;
            }

            foreach (var session in _movedSessions)
            {
                session.PropertyChanged -= MovedSession_PropertyChanged;
            }

            _sessionManager.UnregisterSessionNotification(this);
        }

        private void CreateAndAddSession(IAudioSessionControl session)
        {
            try
            {
                if (!_parent.TryGetTarget(out IAudioDevice parent))
                {
                    throw new Exception("Device session parent is invalid but device is still notifying.");
                }

                var newSession = new AudioDeviceSession(parent, session, _dispatcher);
                _dispatcher.BeginInvoke((Action)(() =>
                {
                    if (newSession.State == SessionState.Moved)
                    {
                        _movedSessions.Add(newSession);
                        newSession.PropertyChanged += MovedSession_PropertyChanged;
                    }
                    else if (newSession.State != SessionState.Expired)
                    {
                        AddSession(newSession);
                    }
                }));
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"AudioDeviceSessionCollection CreateAndAddSession {ex}");
            }
        }

        void IAudioSessionNotification.OnSessionCreated(IAudioSessionControl NewSession)
        {
            Trace.WriteLine($"AudioDeviceSessionCollection OnSessionCreated");
            CreateAndAddSession(NewSession);
        }

        private void AddSession(IAudioDeviceSession session)
        {
            Trace.WriteLine($"AudioDeviceSessionCollection AddSession {session.ExeName} {session.Id}");

            session.PropertyChanged += Session_PropertyChanged;

            if (_parent.TryGetTarget(out var parent))
            {
                foreach (AudioDeviceSessionGroup appGroup in _sessions)
                {
                    if (appGroup.AppId == session.AppId)
                    {
                        foreach (AudioDeviceSessionGroup appSessionGroup in appGroup.Sessions)
                        {
                            if (appSessionGroup.GroupingParam == ((IAudioDeviceSessionInternal)session).GroupingParam)
                            {
                                // If there is a session in the same process, inherit safely.
                                // (Avoids a minesweeper ad playing at max volume when app should be muted)
                                session.IsMuted = session.IsMuted || appSessionGroup.IsMuted;
                                appSessionGroup.AddSession(session);
                                return;
                            }
                        }

                        session.IsMuted = session.IsMuted || appGroup.IsMuted;
                        appGroup.AddSession(new AudioDeviceSessionGroup(parent, session));
                        return;
                    }
                }

                _sessions.Add(new AudioDeviceSessionGroup(parent, new AudioDeviceSessionGroup(parent, session)));
            }
        }

        internal void UnHideSessionsForProcessId(int processId)
        {
            foreach (var session in _movedSessions.ToArray())  // Use snapshot since enumeration will be modified.
            {
                if (session.ProcessId == processId)
                {
                    _movedSessions.Remove(session);
                    session.PropertyChanged -= MovedSession_PropertyChanged;

                    ((IAudioDeviceSessionInternal)session).UnHide();

                    AddSession(session);
                }
            }
        }

        public void MoveHiddenAppsToDevice(string appId, string id)
        {
            foreach (var session in _movedSessions)
            {
                if (session.AppId == appId)
                {
                    ((IAudioDeviceSessionInternal)session).MoveToDevice(id, false);
                }
            }
        }

        private void RemoveSession(IAudioDeviceSession session)
        {
            Trace.WriteLine($"AudioDeviceSessionCollection RemoveSession {session.ExeName} {session.Id}");

            session.PropertyChanged -= Session_PropertyChanged;

            foreach (AudioDeviceSessionGroup appGroup in _sessions)
            {
                foreach (AudioDeviceSessionGroup appSessionGroup in appGroup.Sessions)
                {
                    if (appSessionGroup.Sessions.Contains(session))
                    {
                        appSessionGroup.RemoveSession(session);

                        // Delete the now-empty app session group.
                        if (!appSessionGroup.Sessions.Any())
                        {
                            appGroup.RemoveSession(appSessionGroup);
                            break;
                        }
                    }
                }

                // Delete the now-empty app.
                if (!appGroup.Sessions.Any())
                {
                    _sessions.Remove(appGroup);
                    break;
                }
            }
        }

        private void Session_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var session = (IAudioDeviceSession)sender;

            if (e.PropertyName == nameof(session.State))
            {
                if (session.State == SessionState.Expired)
                {
                    RemoveSession(session);
                }
                else if (session.State == SessionState.Moved)
                {
                    RemoveSession(session);
                    _movedSessions.Add(session);
                    session.PropertyChanged += MovedSession_PropertyChanged;
                }
            }
        }

        private void MovedSession_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var session = (IAudioDeviceSession)sender;

            if (e.PropertyName == nameof(session.State) && session.State == SessionState.Active)
            {
                _movedSessions.Remove(session);
                session.PropertyChanged -= MovedSession_PropertyChanged;

                AddSession(session);
            }
        }
    }
}
