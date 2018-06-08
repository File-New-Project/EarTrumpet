using EarTrumpet.DataModel.Internal.Services;
using EarTrumpet.Interop.MMDeviceAPI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace EarTrumpet.DataModel.Internal
{
    class AudioDeviceSessionCollection : IAudioSessionNotification
    {
        public ObservableCollection<IAudioDeviceSession> Sessions => _sessions;

        private readonly Dispatcher _dispatcher;
        private readonly ObservableCollection<IAudioDeviceSession> _sessions = new ObservableCollection<IAudioDeviceSession>();
        private readonly List<IAudioDeviceSession> _movedSessions = new List<IAudioDeviceSession>();
        private IAudioSessionManager2 _sessionManager;
        private WeakReference<IAudioDevice> _parent;

        public AudioDeviceSessionCollection(IAudioDevice parent, IMMDevice device)
        {
            Trace.WriteLine($"AudioDeviceSessionCollection Create dev={device.GetId()}");

            _parent = new WeakReference<IAudioDevice>(parent);
            _dispatcher = App.Current.Dispatcher;

            Task.Factory.StartNew(() =>
            {
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
                catch(Exception ex)
                {
                    AppTrace.LogWarning(ex);
                }
            });
        }

        ~AudioDeviceSessionCollection()
        {
            foreach (var session in _sessions)
            {
                session.PropertyChanged -= Session_PropertyChanged;
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

                var newSession = new AudioDeviceSession(parent, session);
                _dispatcher.BeginInvoke((Action)(() =>
                {
                    if (newSession.State == SessionState.Moved)
                    {
                        _movedSessions.Add(newSession);
                        newSession.PropertyChanged += MovedSession_PropertyChanged;
                    }
                    else
                    {
                        AddSession(newSession);
                    }
                }));
            }
            catch (ZombieProcessException ex)
            {
                Trace.TraceError($"{ex}");
            }
            catch (Exception ex)
            {
                Trace.TraceError($"{ex}");
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

            foreach (AudioDeviceSessionGroup appGroup in _sessions)
            {
                if (appGroup.ExeName == session.ExeName)
                {
                    foreach (AudioDeviceSessionGroup appSessionGroup in appGroup.Sessions)
                    {
                        if (appSessionGroup.GroupingParam == session.GroupingParam)
                        {
                            // If there is a session in the same process, inherit safely.
                            // (Avoids a minesweeper ad playing at max volume when app should be muted)
                            session.IsMuted = session.IsMuted || appSessionGroup.IsMuted;
                            appSessionGroup.AddSession(session);
                            return;
                        }
                    }

                    session.IsMuted = session.IsMuted || appGroup.IsMuted;
                    appGroup.AddSession(new AudioDeviceSessionGroup(session));
                    return;
                }
            }

            _sessions.Add(new AudioDeviceSessionGroup(new AudioDeviceSessionGroup(session)));
        }

        internal void UnHideSessionsForProcessId(int processId)
        {
            foreach(var session in _movedSessions.ToArray())
            {
                if (session.ProcessId == processId)
                {
                    _movedSessions.Remove(session);
                    session.PropertyChanged -= MovedSession_PropertyChanged;

                    session.UnHide();

                    AddSession(session);
                }
            }
        }

        internal void MoveHiddenAppsToDevice(string appId, string id)
        {
            foreach(var session in _movedSessions)
            {
                if (session.AppId == appId)
                {
                    session.MoveToDevice(id, false);
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
