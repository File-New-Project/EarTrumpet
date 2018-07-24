using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace EarTrumpet_Actions.DataModel.Triggers
{
    public enum AudioDeviceEventTriggerType
    {
        Added,
        Removed,
        BecomingDefault,
        LeavingDefault,
    }

    public class AudioDeviceEventTrigger : BaseTrigger
    {
        public Device Device { get; set; }
        public AudioDeviceEventTriggerType TriggerType { get; set; }

        public AudioDeviceEventTrigger()
        {
            DisplayName = "When an audio device is (added, removed, becomes or leaves default)";
            Options = new List<Option>
            {
                new Option("is added", AudioDeviceEventTriggerType.Added),
                new Option("is removed", AudioDeviceEventTriggerType.Removed),
                new Option("becomes default", AudioDeviceEventTriggerType.BecomingDefault),
                new Option("is no longer default", AudioDeviceEventTriggerType.LeavingDefault),
            };
        }

        public override void Close()
        {

        }

        public override void Loaded()
        {
            var selected = Options.First(o => (AudioDeviceEventTriggerType)o.Value == TriggerType);
            Option = selected.Value;
            DisplayName = $"When {Device} {Option}";
        }
    }
}