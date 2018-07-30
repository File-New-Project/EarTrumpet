using EarTrumpet.DataModel;
using EarTrumpet.Extensibility;
using EarTrumpet.Interop.Helpers;
using EarTrumpet_Actions.DataModel.Triggers;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace EarTrumpet_Actions.DataModel.Processing
{
    class TriggerManager
    {
        public event Action<BaseTrigger> Triggered;

        private List<EventTrigger> _eventTriggers = new List<EventTrigger>();
        private List<AppEventTrigger> _appTriggers = new List<AppEventTrigger>();
        private List<DeviceEventTrigger> _deviceTriggers = new List<DeviceEventTrigger>();
        private IAudioDevice _defaultPlaybackDevice;
        private PlaybackDataModelHost _playbackMgr;

        public TriggerManager()
        {
            _playbackMgr = new PlaybackDataModelHost();
            _playbackMgr.AppPropertyChanged += OnAppPropertyChanged;
            _playbackMgr.AppAdded += (a) => OnAppAddOrRemove(a, AudioAppEventKind.Added);
            _playbackMgr.AppRemoved += (a) => OnAppAddOrRemove(a, AudioAppEventKind.Removed);
            _playbackMgr.DeviceAdded += (d) => OnDeviceAddOrRemove(d, AudioDeviceEventKind.Added);
            _playbackMgr.DeviceRemoved += (d) => OnDeviceAddOrRemove(d, AudioDeviceEventKind.Removed);
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

            _defaultPlaybackDevice = newDefault;
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
                    if ((trigger.Device?.Id == null && device == _playbackMgr.DeviceManager.Default) || 
                         trigger.Device.Id == device.Id)
                    {
                        if (trigger.App.Id == app.Id)
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
                if ((trigger.Device?.Id == null && device == _playbackMgr.DeviceManager.Default) || trigger.Device?.Id == device.Id)
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

        public void OnEvent(ApplicationLifecycleEvent evt)
        {
            foreach (var trigger in _eventTriggers)
            {
                if ((trigger.Option == EarTrumpetEventKind.Startup && evt == ApplicationLifecycleEvent.Startup) ||
                    (trigger.Option == EarTrumpetEventKind.Shutdown && evt == ApplicationLifecycleEvent.Shutdown))
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
            }
            else if (trig is EventTrigger)
            {
                _eventTriggers.Add((EventTrigger)trig);
            }
            else if (trig is DeviceEventTrigger)
            {
                _deviceTriggers.Add((DeviceEventTrigger)trig);
            }
            else if (trig is AppEventTrigger)
            {
                _appTriggers.Add((AppEventTrigger)trig);
            }
            else if (trig is HotkeyTrigger)
            {
                var trigger = (HotkeyTrigger)trig;

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
