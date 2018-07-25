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

    public class AudioDeviceSessionEventTrigger : BaseTrigger, IPartWithDevice, IPartWithApp
    {
        public Device Device { get; set; }
        public App DeviceSession { get; set; }
        public AudioDeviceSessionEventTriggerType TriggerType { get; set; }
        
        public AudioDeviceSessionEventTrigger()
        {
            Description = "When an app session is (added, removed, plays sound, ...)";
            Options = new List<OptionData>(new OptionData[]{ new OptionData(new List<Option>
                {
                    new Option("is added", AudioDeviceSessionEventTriggerType.Added),
                    new Option("is removed", AudioDeviceSessionEventTriggerType.Removed),
                    new Option("is muted", AudioDeviceSessionEventTriggerType.Muted),
                    new Option("is unmuted", AudioDeviceSessionEventTriggerType.Unmuted),
                    new Option("begins playing sound", AudioDeviceSessionEventTriggerType.PlayingSound),
                    new Option("stops playing sound", AudioDeviceSessionEventTriggerType.NotPlayingSound),
                },
                (newValue) => TriggerType = (AudioDeviceSessionEventTriggerType)newValue.Value,
                () => TriggerType) });


        }


        public override string Describe() => $"When {DeviceSession} on {Device} {Options[0].DisplayName}";

    }
}
