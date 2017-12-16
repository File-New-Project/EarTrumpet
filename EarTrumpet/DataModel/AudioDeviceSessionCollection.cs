using EarTrumpet.Extensions;
using EarTrumpet.DataModel.Com;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;

namespace EarTrumpet.DataModel
{
    public class AudioDeviceSessionCollection : IAudioSessionNotification
    {
        Dispatcher _dispatcher;
        ObservableCollection<IAudioDeviceSession> _sessions = new ObservableCollection<IAudioDeviceSession>();

        public AudioDeviceSessionCollection(IMMDevice device, Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;

            var sessionManager = device.Activate<IAudioSessionManager2>();
            sessionManager.RegisterSessionNotification(this);

            // Enumerate existing sessions.
            var enumerator = sessionManager.GetSessionEnumerator();
            int count;
            enumerator.GetCount(out count);
            for (int i = 0; i < count; i++)
            {
                IAudioSessionControl session;
                enumerator.GetSession(i, out session);
                AddSession(new SafeAudioDeviceSession(new AudioDeviceSession(session, _dispatcher)));
            }
        }

        ~AudioDeviceSessionCollection()
        {
            foreach(var session in _sessions)
            {
                session.PropertyChanged -= Session_PropertyChanged;
            }
        }

        public ObservableCollection<IAudioDeviceSession> Sessions => _sessions;

        void IAudioSessionNotification.OnSessionCreated(IAudioSessionControl NewSession)
        {
            _dispatcher.SafeInvoke(() =>
            {
                AddSession(new SafeAudioDeviceSession(new AudioDeviceSession(NewSession, _dispatcher)));
            });
        }

        void AddSession(IAudioDeviceSession session)
        {
            foreach(AudioDeviceSessionContainer container in _sessions)
            {
                if (container.GroupingParam == session.GroupingParam)
                {
                    container.AddSession(session);
                    session.PropertyChanged += Session_PropertyChanged;
                    return;
                }
            }

            var newSession = new AudioDeviceSessionContainer(session);

            // If there is a session in the same process, inherit safely.
            // (Avoids a minesweeper ad playing at max volume when app should be muted)
            foreach (AudioDeviceSessionContainer container in _sessions)
            {
                if (container.ProcessId == newSession.ProcessId)
                {
                    newSession.IsMuted = newSession.IsMuted || container.IsMuted;
                    break;
                }
            }

            _sessions.Add(newSession);
        }

        void RemoveSession(IAudioDeviceSession session)
        {
            foreach (AudioDeviceSessionContainer container in _sessions)
            {
                if (container.Sessions.Contains(session))
                {
                    container.RemoveSession(session);
                    session.PropertyChanged -= Session_PropertyChanged;

                    // Delete the now-empty container.
                    if (!container.Sessions.Any())
                    {
                        _sessions.Remove(container);
                    }

                    return;
                }
            }
            throw new Exception("RemoveSession: session not found");
        }

        private void Session_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var session = (IAudioDeviceSession)sender;

            if (e.PropertyName == nameof(session.GroupingParam))
            {
                RemoveSession(session);
                AddSession(session);
            }
        }
    }
}
