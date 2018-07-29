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

        public TriggerManager()
        {
            PlaybackDataModelHost.AppPropertyChanged += PlaybackDataModelHost_AppPropertyChanged;
            PlaybackDataModelHost.AppAdded += PlaybackDataModelHost_AppAdded;
            PlaybackDataModelHost.AppRemoved += PlaybackDataModelHost_AppRemoved;

            PlaybackDataModelHost.DeviceAdded += PlaybackDataModelHost_DeviceAdded;
            PlaybackDataModelHost.DeviceRemoved += PlaybackDataModelHost_DeviceRemoved;
            PlaybackDataModelHost.DevicePropertyChanged += PlaybackDataModelHost_DevicePropertyChanged;
            PlaybackDataModelHost.DeviceManager.DefaultChanged += PlaybackDeviceManager_DefaultChanged;
            _defaultPlaybackDevice = PlaybackDataModelHost.DeviceManager.Default;
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
                    if (trigger.TriggerType == AudioDeviceEventTriggerType.BecomingDefault)
                    {
                        Triggered?.Invoke(trigger);
                    }
                }

                if (_defaultPlaybackDevice?.Id == Device.AnyDevice.Id || trigger.Device.Id == _defaultPlaybackDevice?.Id)
                {
                    if (trigger.TriggerType == AudioDeviceEventTriggerType.LeavingDefault)
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
                if (trigger.TriggerType == AudioDeviceEventTriggerType.Removed)
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
                if (trigger.TriggerType == AudioDeviceEventTriggerType.Added)
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
                if (trigger.TriggerType == AudioDeviceSessionEventTriggerType.Removed)
                {
                    var device = app.Parent;
                    if (device.Id == Device.AnyDevice.Id || trigger.Device.Id == device.Id || 
                        (trigger.Device.Id == null && device == PlaybackDataModelHost.DeviceManager.Default))
                    {
                        if (app.Id == App.AnySession.Id || trigger.DeviceSession.Id == app.Id)
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
                if (trigger.TriggerType == AudioDeviceSessionEventTriggerType.Added)
                {
                    var device = app.Parent;
                    if (device.Id == Device.AnyDevice.Id || trigger.Device.Id == device.Id ||
                        (trigger.Device.Id == null && device == PlaybackDataModelHost.DeviceManager.Default))
                    {
                        if (app.Id == App.AnySession.Id || trigger.DeviceSession.Id == app.Id)
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
                    (trigger.Device.Id == null && device == PlaybackDataModelHost.DeviceManager.Default))
                {
                    if (app.AppId == App.AnySession.Id || trigger.DeviceSession.Id == app.AppId)
                    {
                        switch (trigger.TriggerType)
                        {
                            case AudioDeviceSessionEventTriggerType.Muted:
                                if (propertyName == nameof(app.IsMuted)
                                    && app.IsMuted)
                                {
                                    Triggered?.Invoke(trigger);
                                }
                                break;
                            case AudioDeviceSessionEventTriggerType.Unmuted:
                                if (propertyName == nameof(app.IsMuted)
                                    && !app.IsMuted)
                                {
                                    Triggered?.Invoke(trigger);
                                }
                                break;
                            case AudioDeviceSessionEventTriggerType.PlayingSound:
                                if (propertyName == nameof(app.State)
                                    && app.State == EarTrumpet.DataModel.SessionState.Active)
                                {
                                    Triggered?.Invoke(trigger);
                                }
                                break;
                            case AudioDeviceSessionEventTriggerType.NotPlayingSound:
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
                if ((trigger.TriggerType == EventTriggerType.EarTrumpet_Startup && 
                    evt == EarTrumpet.Extensibility.ApplicationLifecycleEvent.Startup) ||
                    (trigger.TriggerType == EventTriggerType.EarTrumpet_Shutdown &&
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

                if (trigger.ConditionType == ProcessTriggerConditionType.Starts)
                {
                    Addon.Current.ProcessWatcher.ProcessStarted += triggerIfApplicable;
                }
                else
                {
                    Addon.Current.ProcessWatcher.ProcessStopped += triggerIfApplicable;
                }

                bool isRunning = Addon.Current.ProcessWatcher.ProcessNames.Contains(trigger.Text);
                if (isRunning && trigger.ConditionType == ProcessTriggerConditionType.Starts ||
                    !isRunning && trigger.ConditionType == ProcessTriggerConditionType.Stops)
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
                Trace.WriteLine($"HOTKEY: {trigger.Hotkey}");

                HotkeyManager.Current.Register(trigger.Hotkey);
                HotkeyManager.Current.KeyPressed += (data) =>
                {
                    if (data.Equals(trigger.Hotkey))
                    {
                        Trace.WriteLine($"HOTKEY-TRIGGER: {trigger.Hotkey}");
                        Triggered?.Invoke(trig);
                    }
                };
            }
            else throw new NotImplementedException();
        }
    }
}
