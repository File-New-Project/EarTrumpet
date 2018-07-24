using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Serialization;

namespace EarTrumpet_Actions.DataModel.Triggers
{
    public enum AudioDeviceSessionEventTriggerType
    {
        Added,
        Removed,
        PlayingSound,
        NotPlayingSound,
        Muted,
        Unmuted,
    }

    public class AudioDeviceSessionEventTrigger : BaseTrigger
    {
        public Device Device { get; set; }
        public App DeviceSession { get; set; }
        public AudioDeviceSessionEventTriggerType TriggerType { get; set; }
        
        public AudioDeviceSessionEventTrigger()
        {
            DisplayName = "When an app session is (added, removed, plays sound, ...)";
            Options =  new List<Option>
            {
                new Option("is added", AudioDeviceSessionEventTriggerType.Added),
                new Option("is removed", AudioDeviceSessionEventTriggerType.Removed),
                new Option("is muted", AudioDeviceSessionEventTriggerType.Muted),
                new Option("is unmuted", AudioDeviceSessionEventTriggerType.Unmuted),
                new Option("begins playing sound", AudioDeviceSessionEventTriggerType.PlayingSound),
                new Option("stops playing sound", AudioDeviceSessionEventTriggerType.NotPlayingSound),
            };

            PlaybackDataModelHost.AppPropertyChanged += PlaybackDataModelHost_AppPropertyChanged;
            PlaybackDataModelHost.AppAdded += PlaybackDataModelHost_AppAdded;
            PlaybackDataModelHost.AppRemoved += PlaybackDataModelHost_AppRemoved;
        }

        private void PlaybackDataModelHost_AppRemoved(EarTrumpet.DataModel.IAudioDeviceSession app)
        {
            if (DeviceSession == null || app.AppId == DeviceSession.Id || DeviceSession.Id == App.AnySession.Id)
            {
                // TODO: check device, add Parent property to device session to enable this

                switch (this.TriggerType)
                {
                    case AudioDeviceSessionEventTriggerType.Removed:
                        OnTriggered();
                        break;
                }
            }
        }

        private void PlaybackDataModelHost_AppAdded(EarTrumpet.DataModel.IAudioDeviceSession app)
        {
            if (DeviceSession == null || app.AppId == DeviceSession.Id || DeviceSession.Id == App.AnySession.Id)
            {
                // TODO: check device, add Parent property to device session to enable this

                switch (this.TriggerType)
                {
                    case AudioDeviceSessionEventTriggerType.Added:
                        OnTriggered();
                        break;
                    case AudioDeviceSessionEventTriggerType.PlayingSound:
                        if (app.State == EarTrumpet.DataModel.SessionState.Active)
                        {
                            OnTriggered();
                        }
                        break;
                }
            }
        }

        private void PlaybackDataModelHost_AppPropertyChanged(EarTrumpet.DataModel.IAudioDeviceSession app, string propertyName)
        {
            if (DeviceSession == null || app.AppId == DeviceSession.Id || DeviceSession.Id == App.AnySession.Id)
            {
                // TODO: check device, add Parent property to device session to enable this

                switch (this.TriggerType)
                {
                    case AudioDeviceSessionEventTriggerType.Muted:
                        if (propertyName == nameof(app.IsMuted)
                            && app.IsMuted)
                        {
                            OnTriggered();
                        }
                        break;
                    case AudioDeviceSessionEventTriggerType.Unmuted:
                        if (propertyName == nameof(app.IsMuted)
                            && !app.IsMuted)
                        {
                            OnTriggered();
                        }
                        break;
                    case AudioDeviceSessionEventTriggerType.PlayingSound:
                        if (propertyName == nameof(app.State)
                            && app.State == EarTrumpet.DataModel.SessionState.Active)
                        {
                            OnTriggered();
                        }
                        break;
                    case AudioDeviceSessionEventTriggerType.NotPlayingSound:
                        if (propertyName == nameof(app.State)
                            && app.State != EarTrumpet.DataModel.SessionState.Active)
                        {
                            OnTriggered();
                        }
                        break;
                }
            }
        }

        public override void Close()
        {

        }

        public override void Loaded()
        {
            var selected = Options.First(o => (AudioDeviceSessionEventTriggerType)o.Value == TriggerType);
            Option = selected.Value;
            DisplayName = $"When {DeviceSession} on {Device} {Option}";
        }
    }
}
