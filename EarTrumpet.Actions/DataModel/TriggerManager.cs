using EarTrumpet.Interop.Helpers;
using EarTrumpet_Actions.DataModel.Triggers;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace EarTrumpet_Actions.DataModel
{
    class TriggerManager
    {
        public event Action<BaseTrigger> Triggered;

        private List<EventTrigger> _eventTriggers = new List<EventTrigger>();
        private List<AudioDeviceSessionEventTrigger> _appTriggers = new List<AudioDeviceSessionEventTrigger>();
        private List<AudioDeviceEventTrigger> _deviceTriggers = new List<AudioDeviceEventTrigger>();
        private EarTrumpet.DataModel.IAudioDevice _defaultPlaybackDevice;
        private PlaybackDataModelHost _playbackMgr;

        public TriggerManager()
        {
            _playbackMgr = new PlaybackDataModelHost();
            _playbackMgr.AppPropertyChanged += PlaybackDataModelHost_AppPropertyChanged;
            _playbackMgr.AppAdded += PlaybackDataModelHost_AppAdded;
            _playbackMgr.AppRemoved += PlaybackDataModelHost_AppRemoved;
            _playbackMgr.DeviceAdded += PlaybackDataModelHost_DeviceAdded;
            _playbackMgr.DeviceRemoved += PlaybackDataModelHost_DeviceRemoved;
            _playbackMgr.DevicePropertyChanged += PlaybackDataModelHost_DevicePropertyChanged;
            _playbackMgr.DeviceManager.DefaultChanged += PlaybackDeviceManager_DefaultChanged;
            _defaultPlaybackDevice = _playbackMgr.DeviceManager.Default;
        }

        public void Clear()
        {
            _appTriggers.Clear();
            _eventTriggers.Clear();
            _deviceTriggers.Clear();
        }

        private void PlaybackDeviceManager_DefaultChanged(object sender, EarTrumpet.DataModel.IAudioDevice newDefault)
        {
            if (newDefault == null) return;

            foreach (var trigger in _deviceTriggers)
            {
                if (newDefault.Id == Device.AnyDevice.Id || trigger.Device.Id == newDefault.Id)
                {
                    if (trigger.Option == AudioDeviceEventKind.BecomingDefault)
                    {
                        Triggered?.Invoke(trigger);
                    }
                }

                if (_defaultPlaybackDevice?.Id == Device.AnyDevice.Id || trigger.Device.Id == _defaultPlaybackDevice?.Id)
                {
                    if (trigger.Option == AudioDeviceEventKind.LeavingDefault)
                    {
                        Triggered?.Invoke(trigger);
                    }
                }
            }

            _defaultPlaybackDevice = newDefault;
        }

        private void PlaybackDataModelHost_DevicePropertyChanged(EarTrumpet.DataModel.IAudioDevice arg1, string arg2)
        {
            // Muted, Unumuted, Volume changed
        }

        private void PlaybackDataModelHost_DeviceRemoved(EarTrumpet.DataModel.IAudioDevice oldDevice)
        {
            foreach (var trigger in _deviceTriggers)
            {
                if (trigger.Option == AudioDeviceEventKind.Removed)
                {
                    // Default device: not supported
                    if (oldDevice.Id == Device.AnyDevice.Id ||
                        trigger.Device.Id == oldDevice.Id)
                    {
                        Triggered?.Invoke(trigger);
                    }
                }
            }
        }

        private void PlaybackDataModelHost_DeviceAdded(EarTrumpet.DataModel.IAudioDevice newDevice)
        {
            foreach(var trigger in _deviceTriggers)
            {
                if (trigger.Option == AudioDeviceEventKind.Added)
                {
                    // Default device: not supported
                    if (newDevice.Id == Device.AnyDevice.Id ||
                        trigger.Device.Id == newDevice.Id)
                    {
                        Triggered?.Invoke(trigger);
                    }
                }
            }
        }

        private void PlaybackDataModelHost_AppRemoved(EarTrumpet.DataModel.IAudioDeviceSession app)
        {
            foreach (var trigger in _appTriggers)
            {
                if (trigger.Option == AudioAppEventKind.Removed)
                {
                    var device = app.Parent;
                    if (device.Id == Device.AnyDevice.Id || trigger.Device.Id == device.Id || 
                        (trigger.Device.Id == null && device == _playbackMgr.DeviceManager.Default))
                    {
                        if (app.Id == App.AnySession.Id || trigger.App.Id == app.Id)
                        {
                            Triggered?.Invoke(trigger);
                        }
                    }
                }
            }
        }

        private void PlaybackDataModelHost_AppAdded(EarTrumpet.DataModel.IAudioDeviceSession app)
        {
            foreach (var trigger in _appTriggers)
            {
                if (trigger.Option == AudioAppEventKind.Added)
                {
                    var device = app.Parent;
                    if (device.Id == Device.AnyDevice.Id || trigger.Device.Id == device.Id ||
                        (trigger.Device.Id == null && device == _playbackMgr.DeviceManager.Default))
                    {
                        if (app.Id == App.AnySession.Id || trigger.App.Id == app.Id)
                        {
                            Triggered?.Invoke(trigger);
                        }
                    }
                }
            }
        }

        private void PlaybackDataModelHost_AppPropertyChanged(EarTrumpet.DataModel.IAudioDeviceSession app, string propertyName)
        {
            foreach (var trigger in _appTriggers)
            {
                var device = app.Parent;
                if (device.Id == Device.AnyDevice.Id || trigger.Device.Id == device.Id ||
                    (trigger.Device.Id == null && device == _playbackMgr.DeviceManager.Default))
                {
                    if (app.AppId == App.AnySession.Id || trigger.App.Id == app.AppId)
                    {
                        switch (trigger.Option)
                        {
                            case AudioAppEventKind.Muted:
                                if (propertyName == nameof(app.IsMuted)
                                    && app.IsMuted)
                                {
                                    Triggered?.Invoke(trigger);
                                }
                                break;
                            case AudioAppEventKind.Unmuted:
                                if (propertyName == nameof(app.IsMuted)
                                    && !app.IsMuted)
                                {
                                    Triggered?.Invoke(trigger);
                                }
                                break;
                            case AudioAppEventKind.PlayingSound:
                                if (propertyName == nameof(app.State)
                                    && app.State == EarTrumpet.DataModel.SessionState.Active)
                                {
                                    Triggered?.Invoke(trigger);
                                }
                                break;
                            case AudioAppEventKind.NotPlayingSound:
                                if (propertyName == nameof(app.State)
                                    && app.State != EarTrumpet.DataModel.SessionState.Active)
                                {
                                    Triggered?.Invoke(trigger);
                                }
                                break;
                        }
                    }
                }
            }
        }

        public void OnEvent(EarTrumpet.Extensibility.ApplicationLifecycleEvent evt)
        {
            foreach(var trigger in _eventTriggers)
            {
                if ((trigger.Option == EarTrumpetEventKind.Startup && 
                    evt == EarTrumpet.Extensibility.ApplicationLifecycleEvent.Startup) ||
                    (trigger.Option == EarTrumpetEventKind.Shutdown &&
                    evt == EarTrumpet.Extensibility.ApplicationLifecycleEvent.Shutdown))
                {
                    Triggered?.Invoke(trigger);
                }
            }
        }

        public void Register(BaseTrigger trig)
        {
            if (trig is ProcessTrigger)
            {
                var trigger = (ProcessTrigger)trig;

                var triggerIfApplicable = new Action<string>((name) =>
                {
                    if (name == trigger.Text)
                    {
                        Triggered?.Invoke(trig);
                    }
                });

                if (trigger.Option == ProcessEventKind.Start)
                {
                    ProcessWatcher.Current.ProcessStarted += triggerIfApplicable;
                }
                else
                {
                    ProcessWatcher.Current.ProcessStopped += triggerIfApplicable;
                }

                bool isRunning = ProcessWatcher.Current.ProcessNames.Contains(trigger.Text);
                if (isRunning && trigger.Option == ProcessEventKind.Start ||
                    !isRunning && trigger.Option == ProcessEventKind.Stop)
                {
                    triggerIfApplicable(trigger.Text);
                }
            }
            else if (trig is EventTrigger)
            {
                _eventTriggers.Add((EventTrigger)trig);
            }
            else if (trig is AudioDeviceEventTrigger)
            {
                _deviceTriggers.Add((AudioDeviceEventTrigger)trig);
            }
            else if (trig is AudioDeviceSessionEventTrigger)
            {
                _appTriggers.Add((AudioDeviceSessionEventTrigger)trig);
            }
            else if (trig is HotkeyTrigger)
            {
                var trigger = (HotkeyTrigger)trig;
                Trace.WriteLine($"HOTKEY: {trigger.Option}");

                HotkeyManager.Current.Register(trigger.Option);
                HotkeyManager.Current.KeyPressed += (data) =>
                {
                    if (data.Equals(trigger.Option))
                    {
                        Trace.WriteLine($"HOTKEY-TRIGGER: {trigger.Option}");
                        Triggered?.Invoke(trig);
                    }
                };
            }
            else throw new NotImplementedException();
        }
    }
}
