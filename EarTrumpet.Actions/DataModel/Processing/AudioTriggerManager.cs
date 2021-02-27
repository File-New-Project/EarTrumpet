using EarTrumpet.Actions.DataModel.Enum;
using EarTrumpet.Actions.DataModel.Serialization;
using System;
using System.Collections.Generic;
using EarTrumpet.DataModel.Audio;
using EarTrumpet.DataModel.WindowsAudio;
using EarTrumpet.Extensibility.Shared;

namespace EarTrumpet.Actions.DataModel.Processing
{
    class AudioTriggerManager
    {
        public event Action<BaseTrigger> Triggered;

        private readonly PlaybackDataModelHost _playbackManager;
        private readonly IAudioDeviceManager _recordingManager;
        private readonly List<AppEventTrigger> _appTriggers = new List<AppEventTrigger>();
        private readonly List<DeviceEventTrigger> _deviceTriggers = new List<DeviceEventTrigger>();
        private IAudioDevice _defaultPlaybackDevice;
        private IAudioDevice _defaultRecordingDevice;

        public AudioTriggerManager()
        {
            _playbackManager = PlaybackDataModelHost.Current;
            _playbackManager.AppPropertyChanged += OnAppPropertyChanged;
            _playbackManager.AppAdded += (a) => OnAppAddOrRemove(a, AudioAppEventKind.Added);
            _playbackManager.AppRemoved += (a) => OnAppAddOrRemove(a, AudioAppEventKind.Removed);
            _playbackManager.DeviceAdded += (d) => OnDeviceAddOrRemove(d, AudioDeviceEventKind.Added);
            _playbackManager.DeviceRemoved += (d) => OnDeviceAddOrRemove(d, AudioDeviceEventKind.Removed);
            _playbackManager.DeviceManager.DefaultChanged += PlaybackDeviceManager_DefaultChanged;
            _defaultPlaybackDevice = _playbackManager.DeviceManager.Default;

            _recordingManager = WindowsAudioFactory.Create(AudioDeviceKind.Recording);
            _recordingManager.DefaultChanged += RecordingMgr_DefaultChanged;
            _defaultRecordingDevice = _recordingManager.Default;
        }

        public void Register(BaseTrigger trigger)
        {
            if (trigger is DeviceEventTrigger)
            {
                _deviceTriggers.Add((DeviceEventTrigger)trigger);
            }
            else if (trigger is AppEventTrigger)
            {
                _appTriggers.Add((AppEventTrigger)trigger);
            }
            else throw new NotImplementedException();
        }

        public void Clear()
        {
            _appTriggers.Clear();
            _deviceTriggers.Clear();
        }

        private void PlaybackDeviceManager_DefaultChanged(object sender, EarTrumpet.DataModel.Audio.IAudioDevice newDefault)
        {
            if (newDefault == null) return;

            ProcessDefaultChanged(newDefault);

            _defaultPlaybackDevice = newDefault;
        }

        private void RecordingMgr_DefaultChanged(object sender, IAudioDevice newDefault)
        {
            if (newDefault == null) return;

            ProcessDefaultChanged(newDefault);

            _defaultRecordingDevice = newDefault;
        }

        private void ProcessDefaultChanged(IAudioDevice newDefault)
        {
            foreach (var trigger in _deviceTriggers)
            {
                if (trigger.Device.Id == _defaultPlaybackDevice?.Id &&
                    trigger.Option == AudioDeviceEventKind.LeavingDefault)
                {
                    Triggered?.Invoke(trigger);
                }

                if (trigger.Device.Id == newDefault.Id &&
                    trigger.Option == AudioDeviceEventKind.BecomingDefault)
                {
                    Triggered?.Invoke(trigger);
                }
            }
        }

        private void OnDeviceAddOrRemove(IAudioDevice device, AudioDeviceEventKind option)
        {
            foreach (var trigger in _deviceTriggers)
            {
                if (trigger.Option == option)
                {
                    // Default device: not supported
                    if (trigger.Device.Id == device.Id)
                    {
                        Triggered?.Invoke(trigger);
                    }
                }
            }
        }

        private void OnAppAddOrRemove(IAudioDeviceSession app, AudioAppEventKind option)
        {
            foreach (var trigger in _appTriggers)
            {
                if (trigger.Option == option)
                {
                    var device = app.Parent;
                    if ((trigger.Device?.Id == null && device == _playbackManager.DeviceManager.Default) ||
                         trigger.Device?.Id == device.Id)
                    {
                        if (trigger.App.Id == app.AppId)
                        {
                            Triggered?.Invoke(trigger);
                        }
                    }
                }
            }
        }

        private void OnAppPropertyChanged(IAudioDeviceSession app, string propertyName)
        {
            foreach (var trigger in _appTriggers)
            {
                var device = app.Parent;
                if ((trigger.Device?.Id == null && device == _playbackManager.DeviceManager.Default) || trigger.Device?.Id == device.Id)
                {
                    if (trigger.App.Id == app.AppId)
                    {
                        switch (trigger.Option)
                        {
                            case AudioAppEventKind.Muted:
                                if (propertyName == nameof(app.IsMuted) && app.IsMuted)
                                {
                                    Triggered?.Invoke(trigger);
                                }
                                break;
                            case AudioAppEventKind.Unmuted:
                                if (propertyName == nameof(app.IsMuted) && !app.IsMuted)
                                {
                                    Triggered?.Invoke(trigger);
                                }
                                break;
                            case AudioAppEventKind.PlayingSound:
                                if (propertyName == nameof(app.State) && app.State == SessionState.Active)
                                {
                                    Triggered?.Invoke(trigger);
                                }
                                break;
                            case AudioAppEventKind.NotPlayingSound:
                                if (propertyName == nameof(app.State) && app.State != SessionState.Active)
                                {
                                    Triggered?.Invoke(trigger);
                                }
                                break;
                        }
                    }
                }
            }
        }
    }
}
