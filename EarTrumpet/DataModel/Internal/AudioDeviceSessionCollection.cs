using EarTrumpet.DataModel.Internal.Services;
using EarTrumpet.Extensions;
using EarTrumpet.Interop.MMDeviceAPI;
using EarTrumpet.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Threading;

namespace EarTrumpet.DataModel.Internal
{
    class AudioDeviceSessionCollection : IAudioSessionNotification
    {
        public ObservableCollection<IAudioDeviceSession> Sessions => _sessions;

        private Dispatcher _dispatcher;
        private ObservableCollection<IAudioDeviceSession> _sessions = new ObservableCollection<IAudioDeviceSession>();
        private List<IAudioDeviceSession> _movedSessions = new List<IAudioDeviceSession>();

        public AudioDeviceSessionCollection(IMMDevice device)
        {
            _dispatcher = App.Current.Dispatcher;

            var sessionManager = device.Activate<IAudioSessionManager2>();
            sessionManager.RegisterSessionNotification(this);

            var enumerator = sessionManager.GetSessionEnumerator();
            int count = enumerator.GetCount();
            for (int i = 0; i < count; i++)
            {
                CreateAndAddSession(enumerator.GetSession(i));
            }
        }

        ~AudioDeviceSessionCollection()
        {
            foreach(var session in _sessions)
            {
                session.PropertyChanged -= Session_PropertyChanged;
            }
        }

        private void CreateAndAddSession(IAudioSessionControl session)
        {
            try
            {
                AddSession(new SafeAudioDeviceSession(new AudioDeviceSession(session)));
            }
            catch(COMException ex)
            {
                Debug.WriteLine(ex);
            }
            catch (ZombieProcessException)
            {
                // Nothing to do.
            }
        }

        void IAudioSessionNotification.OnSessionCreated(IAudioSessionControl NewSession)
        {
            _dispatcher.SafeInvoke(() => CreateAndAddSession(NewSession));
        }

        private void AddSession(IAudioDeviceSession session)
        {
            session.PropertyChanged += Session_PropertyChanged;

            foreach (AudioDeviceSessionGroup appGroup in _sessions)
            {
                if (appGroup.AppId == session.AppId)
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

        private void RemoveSession(IAudioDeviceSession session)
        {
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
