using System.Collections.Generic;

namespace EarTrumpet_Actions.DataModel.Triggers
{
    public enum AudioDeviceEventKind
    {
        Added,
        Removed,
        BecomingDefault,
        LeavingDefault,
    }

    public class DeviceEventTrigger : BaseTrigger, IPartWithDevice
    {
        public Device Device { get; set; }
        public AudioDeviceEventKind Option { get; set; }

        public override string Describe() => $"When {Device} {Options[0].DisplayName}";

        public DeviceEventTrigger()
        {
            Description = "When an audio device is (added, removed, becomes or leaves default)";
            Options = new List<OptionData>(new OptionData[]{ new OptionData(new List<Option>
                {
                    new Option("is added", AudioDeviceEventKind.Added),
                    new Option("is removed", AudioDeviceEventKind.Removed),
                    new Option("becomes default", AudioDeviceEventKind.BecomingDefault),
                    new Option("is no longer default", AudioDeviceEventKind.LeavingDefault),
                },
                (newValue) => Option = (AudioDeviceEventKind)newValue.Value,
                () => Option) });
        }
    }
}