using System.Collections.Generic;

namespace EarTrumpet_Actions.DataModel.Triggers
{
    public enum AudioDeviceEventTriggerType
    {
        Added,
        Removed,
        BecomingDefault,
        LeavingDefault,
    }

    public class AudioDeviceEventTrigger : BaseTrigger, IPartWithDevice
    {
        public Device Device { get; set; }
        public AudioDeviceEventTriggerType TriggerType { get; set; }

        public override string Describe() => $"When {Device} {Options[0].DisplayName}";

        public AudioDeviceEventTrigger()
        {
            Description = "When an audio device is (added, removed, becomes or leaves default)";
            Options = new List<OptionData>(new OptionData[]{ new OptionData(new List<Option>
                {
                    new Option("is added", AudioDeviceEventTriggerType.Added),
                    new Option("is removed", AudioDeviceEventTriggerType.Removed),
                    new Option("becomes default", AudioDeviceEventTriggerType.BecomingDefault),
                    new Option("is no longer default", AudioDeviceEventTriggerType.LeavingDefault),
                },
                (newValue) => TriggerType = (AudioDeviceEventTriggerType)newValue.Value,
                () => TriggerType) });
        }
    }
}