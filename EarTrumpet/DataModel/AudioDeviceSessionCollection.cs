using EarTrumpet.Extensions;
using MMDeviceAPI_Interop;
using SoundControlAPI_Interop;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;

namespace EarTrumpet.DataModel
{
    public class AudioDeviceSessionCollection : IAudioSessionNotification, IAudioDeviceSessionCollection
    {
        Dispatcher m_dispatcher;
        ObservableCollection<IAudioDeviceSession> m_sessions = new ObservableCollection<IAudioDeviceSession>();

        public AudioDeviceSessionCollection(IMMDevice device, Dispatcher dispatcher)
        {
            m_dispatcher = dispatcher;

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
                AddSession(new AudioDeviceSession(session, m_dispatcher));
            }
        }

        public ObservableCollection<IAudioDeviceSession> Sessions => m_sessions;

        void IAudioSessionNotification.OnSessionCreated(IAudioSessionControl NewSession)
        {
            m_dispatcher.SafeInvoke(() =>
            {
                var sess = new AudioDeviceSession(NewSession, m_dispatcher);
                AddSession(sess);
            });
        }

        void AddSession(IAudioDeviceSession session)
        {
            foreach(AudioDeviceSessionContainer container in m_sessions)
            {
                if (container.GroupingParam == session.GroupingParam)
                {
                    container.AddSession(session);
                    session.PropertyChanged += Session_PropertyChanged;
                    return;
                }
            }

            m_sessions.Add(new AudioDeviceSessionContainer(session));
        }

        void RemoveSession(IAudioDeviceSession session)
        {
            foreach (AudioDeviceSessionContainer container in m_sessions)
            {
                if (container.Sessions.Contains(session))
                {
                    container.RemoveSession(session);
                    session.PropertyChanged -= Session_PropertyChanged;

                    // Delete the now-empty container.
                    if (!container.Sessions.Any())
                    {
                        m_sessions.Remove(container);
                    }

                    return;
                }
            }
            throw new Exception("RemoveSession: session not found");
        }

        private void Session_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var session = (AudioDeviceSession)sender;

            if (e.PropertyName == "GroupingParam")
            {
                RemoveSession(session);
                AddSession(session);
            }
        }
    }
}
