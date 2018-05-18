using EarTrumpet.Extensions;
using EarTrumpet.Interop;
using EarTrumpet.Interop.MMDeviceAPI;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Threading;

namespace EarTrumpet.DataModel.Internal
{
    public class AudioDeviceSessionCollection : IAudioSessionNotification
    {
        private Dispatcher _dispatcher;
        private ObservableCollection<IAudioDeviceSession> _sessions = new ObservableCollection<IAudioDeviceSession>();
        private IAudioDevice _device;

        public AudioDeviceSessionCollection(IMMDevice device, IAudioDevice audioDevice, Dispatcher dispatcher)
        {
            _device = audioDevice;
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
                AddSession(new SafeAudioDeviceSession(new AudioDeviceSession(session, _device, _dispatcher)));
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
                try
                {
                    AddSession(new SafeAudioDeviceSession(new AudioDeviceSession(NewSession, _device, _dispatcher)));
                }
                catch(COMException ex)
                {
                    Debug.WriteLine(ex);
                }
            });
        }

        private void AddSession(IAudioDeviceSession session)
        {
            foreach(AudioDeviceSessionGroup container in _sessions)
            {
                if (container.AppId == session.AppId)
                {
                    // If there is a session in the same process, inherit safely.
                    // (Avoids a minesweeper ad playing at max volume when app should be muted)
                    session.IsMuted = session.IsMuted || container.IsMuted;

                    container.AddSession(session);
                    session.PropertyChanged += Session_PropertyChanged;
                    return;
                }
            }

            session.PropertyChanged += Session_PropertyChanged;

            var newSession = new AudioDeviceSessionGroup(session);
            _sessions.Add(newSession);
        }

        private void RemoveSession(IAudioDeviceSession session)
        {
            foreach (AudioDeviceSessionGroup container in _sessions)
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

            if (e.PropertyName == nameof(session.State))
            {
                if (session.State == AudioSessionState.Expired)
                {
                    RemoveSession(session);
                }
            }
        }
    }
}
