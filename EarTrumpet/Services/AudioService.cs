using EarTrumpet.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Timers;

namespace EarTrumpet.Services
{
    public class AudioService : IAudioService
    {
        private readonly IAudioDeviceService _audioDeviceService;
        private readonly IAudioSessionService _audioSessionService;
        private readonly Random _random_for_temporary_integration_use;

        private List<AudioDeviceModel> _devices_for_temporary_integration_use;
        private List<AudioSessionModel> _sessions_for_temporary_integration_use;
        
        public event EventHandler<DeviceVolumeChangedArgs> DeviceVolumeChanged;

        public event EventHandler<SessionDisplayNameChangedArgs> SessionDisplayNameChanged;
        public event EventHandler<SessionGroupingChangedArgs> SessionGroupingChanged;
        public event EventHandler<SessionIconChangedArgs> SessionIconChanged;
        public event EventHandler<SessionStateChangedArgs> SessionStateChanged;
        public event EventHandler<SessionVolumeChangedArgs> SessionVolumeChanged;

        public AudioService(IAudioDeviceService audioDeviceService, IAudioSessionService audioSessionService)
        {
            _audioDeviceService = audioDeviceService;
            _audioSessionService = audioSessionService;
            _random_for_temporary_integration_use = new Random();

            StartFakeEventGenerator();
        }

        public IEnumerable<AudioDeviceAndSessionsModel> GetAudioDevicesAndSessions()
        {
            var devices = _audioDeviceService.GetAudioDevices();
            _devices_for_temporary_integration_use = devices.ToList();

            var sessions = _audioSessionService.GetAudioSessions();
            _sessions_for_temporary_integration_use = sessions.ToList();

            return
                from device in devices
                from session in sessions
                where device.Id == session.DeviceId
                group session by device into grp
                select new AudioDeviceAndSessionsModel
                {
                    Id = grp.Key.Id,
                    DisplayName = grp.Key.DisplayName,
                    IsDefault = grp.Key.IsDefault,
                    IsMuted = grp.Key.IsMuted,
                    Sessions = grp.ToList()
                };
        }

        protected virtual void OnDeviceVolumeChanged(DeviceVolumeChangedArgs args)
        {
            DeviceVolumeChanged?.Invoke(this, args);
        }

        protected virtual void OnSessionDisplayNameChange(SessionDisplayNameChangedArgs args)
        {
            SessionDisplayNameChanged?.Invoke(this, args);
        }

        protected virtual void OnSessionGroupingChanged(SessionGroupingChangedArgs args)
        {
            SessionGroupingChanged?.Invoke(this, args);
        }

        protected virtual void OnSessionIconChanged(SessionIconChangedArgs args)
        {
            SessionIconChanged?.Invoke(this, args);
        }

        protected virtual void OnSessionStateChanged(SessionStateChangedArgs args)
        {
            SessionStateChanged?.Invoke(this, args);
        }

        protected virtual void OnSessionVolumeChanged(SessionVolumeChangedArgs args)
        {
            SessionVolumeChanged?.Invoke(this, args);
        }

        private readonly string[] _wordBank = new[]
        {
            "wheel", "enchanting", "profuse", "moor", "bed", "chess", "live", "spot", "fearful",
            "separate", "normal", "demonic", "ossified", "part", "adhesive", "shop", "noisy",
            "sore", "zephyr", "claim", "warm", "kindhearted", "battle", "adjustment", "ray"
        };

        private string GetRandomDisplayName()
        {
            return _wordBank[_random_for_temporary_integration_use.Next(0, _wordBank.Length - 1)] + " " + _wordBank[_random_for_temporary_integration_use.Next(0, _wordBank.Length - 1)];
        }

        private uint GetRandomColor()
        {
            return (uint)Color.FromArgb(_random_for_temporary_integration_use.Next(1, 255), _random_for_temporary_integration_use.Next(1, 255), _random_for_temporary_integration_use.Next(1, 255)).ToArgb();
        }

        private AudioSessionModel GetRandomSession()
        {
            return _sessions_for_temporary_integration_use[_random_for_temporary_integration_use.Next(0, _sessions_for_temporary_integration_use.Count - 1)];
        }

        private AudioDeviceModel GetRandomDevice()
        {
            return _devices_for_temporary_integration_use[_random_for_temporary_integration_use.Next(0, _devices_for_temporary_integration_use.Count - 1)];
        }

        private void StartFakeEventGenerator()
        {
            var eventTimer = new Timer();

            eventTimer.Interval = TimeSpan.FromSeconds(5).TotalMilliseconds;
            eventTimer.Elapsed += (_, __) =>
            {
                if (_devices_for_temporary_integration_use == null || _sessions_for_temporary_integration_use == null)
                    return;

                var randomEvent = _random_for_temporary_integration_use.Next(1, 6);
                switch (randomEvent)
                {
                    case 1:
                        OnSessionDisplayNameChange(new SessionDisplayNameChangedArgs(GetRandomSession().SessionId, GetRandomDisplayName()));
                        break;
                    case 2:
                        OnSessionGroupingChanged(new SessionGroupingChangedArgs(GetRandomSession().SessionId, Guid.NewGuid()));
                        break;
                    case 3:
                        OnSessionIconChanged(new SessionIconChangedArgs(GetRandomSession().SessionId, ""));
                        break;
                    case 4:
                        OnSessionStateChanged(new SessionStateChangedArgs(GetRandomSession().SessionId));
                        break;
                    case 5:
                        OnSessionVolumeChanged(new SessionVolumeChangedArgs(GetRandomSession().SessionId, (float)_random_for_temporary_integration_use.NextDouble(), false));
                        break;
                    case 6:
                        OnDeviceVolumeChanged(new DeviceVolumeChangedArgs(GetRandomDevice().Id, (float)_random_for_temporary_integration_use.NextDouble(), false));
                        break;
                }
            };

            eventTimer.Enabled = true;
        }
    }
}
