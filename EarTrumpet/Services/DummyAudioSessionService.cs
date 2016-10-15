using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Timers;
using EarTrumpet.Models;

namespace EarTrumpet.Services
{
    public class DummyAudioSessionService : IAudioSessionService
    {
        public event EventHandler<DisplayNameChangedArgs> DisplayNameChanged;
        public event EventHandler<GroupingChangedArgs> GroupingChanged;
        public event EventHandler<IconChangedArgs> IconChanged;
        public event EventHandler<SessionStateChangedArgs> SessionStateChanged;
        public event EventHandler<VolumeChangedArgs> VolumeChanged;

        private readonly Random _random;
        private readonly string[] _wordBank = new [] 
        {
            "wheel", "enchanting", "profuse", "moor", "bed", "chess", "live", "spot", "fearful",
            "separate", "normal", "demonic", "ossified", "part", "adhesive", "shop", "noisy",
            "sore", "zephyr", "claim", "warm", "kindhearted", "battle", "adjustment", "ray"
        };

        private List<EarTrumpetAudioSessionModel> _sessions;

        public DummyAudioSessionService()
        {
            _random = new Random();

            GenerateFakeSessions();
            StartFakeEventGenerator();
        }

        private void GenerateFakeSessions()
        {
            var sessionCount = _random.Next(5, 12);
            _sessions = new List<EarTrumpetAudioSessionModel>(sessionCount);

            for (int i = 0; i < sessionCount; i++)
            {
                var model = new EarTrumpetAudioSessionModel();
                model.BackgroundColor = GetRandomColor();
                model.DisplayName = GetRandomDisplayName();
                model.GroupingId = Guid.NewGuid();
                model.IsMuted = false;
                model.ProcessId = (uint)_random.Next(1, 1024);
                model.SessionId = (uint)_random.Next(1, int.MaxValue);
                model.Volume = 1f;

                _sessions.Add(model);
            }
        }

        private string GetRandomDisplayName()
        {
            return _wordBank[_random.Next(0, _wordBank.Length - 1)] + " " + _wordBank[_random.Next(0, _wordBank.Length - 1)];
        }

        private uint GetRandomColor()
        {
            return (uint)Color.FromArgb(_random.Next(1, 255), _random.Next(1, 255), _random.Next(1, 255)).ToArgb();
        }

        private EarTrumpetAudioSessionModel GetRandomSession()
        {
            return _sessions[_random.Next(0, _sessions.Count - 1)];
        }

        private void StartFakeEventGenerator()
        {
            var eventTimer = new Timer();

            eventTimer.Interval = TimeSpan.FromSeconds(5).TotalMilliseconds;
            eventTimer.Elapsed += (_, __) =>
            {
                var randomEvent = _random.Next(1, 5);
                switch(randomEvent)
                {
                    case 1:
                        OnDisplayNameChange(new DisplayNameChangedArgs(GetRandomSession().SessionId, GetRandomDisplayName()));
                        break;
                    case 2:
                        OnGroupingChanged(new GroupingChangedArgs(GetRandomSession().SessionId, Guid.NewGuid()));
                        break;
                    case 3:
                        OnIconChanged(new IconChangedArgs(GetRandomSession().SessionId, ""));
                        break;
                    case 4:
                        OnSessionStateChanged(new SessionStateChangedArgs(GetRandomSession().SessionId));
                        break;
                    case 5:
                        OnVolumeChanged(new VolumeChangedArgs(GetRandomSession().SessionId, (float)_random.NextDouble(), false));
                        break;
                }
            };

            eventTimer.Enabled = true;
        }

        public IEnumerable<EarTrumpetAudioSessionModel> GetAudioSessions()
        {
            return new List<EarTrumpetAudioSessionModel>(_sessions);
        }

        public IEnumerable<EarTrumpetAudioSessionModelGroup> GetAudioSessionGroups()
        {
            return GetAudioSessions().GroupBy(
                x => x.GroupingId,
                x => x, (key, result) => new EarTrumpetAudioSessionModelGroup(result.ToList()));
        }

        public void SetAudioSessionMute(uint sessionId, bool isMuted)
        {
            var session = _sessions.Where(s => s.SessionId == sessionId).First();
            session.IsMuted = true;
        }

        public void SetAudioSessionVolume(uint sessionId, float volume)
        {
            var session = _sessions.FirstOrDefault(s => s.SessionId == sessionId);
            session.IsMuted = false;
            session.Volume = volume;
        }

        protected virtual void OnDisplayNameChange(DisplayNameChangedArgs args)
        {
            _sessions.FirstOrDefault(s => s.SessionId == args.SessionId).DisplayName = args.DisplayName;
            DisplayNameChanged?.Invoke(this, args);
        }

        protected virtual void OnGroupingChanged(GroupingChangedArgs args)
        {
            _sessions.FirstOrDefault(s => s.SessionId == args.SessionId).GroupingId = args.GroupingId;
            GroupingChanged?.Invoke(this, args);
        }

        protected virtual void OnIconChanged(IconChangedArgs args)
        {
            _sessions.FirstOrDefault(s => s.SessionId == args.SessionId).IconPath = args.Icon;
            IconChanged?.Invoke(this, args);
        }

        protected virtual void OnSessionStateChanged(SessionStateChangedArgs args)
        {
            SessionStateChanged?.Invoke(this, args);
        }

        protected virtual void OnVolumeChanged(VolumeChangedArgs args)
        {
            _sessions.FirstOrDefault(s => s.SessionId == args.SessionId).Volume = args.Volume;
            VolumeChanged?.Invoke(this, args);
        }
    }
}
