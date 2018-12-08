using EarTrumpet_Actions.DataModel.Enum;
using System.Collections.Generic;

namespace EarTrumpet_Actions.DataModel.Triggers
{
    public class DeviceEventTrigger : BaseTrigger, IPartWithDevice
    {
        public Device Device { get; set; }
        public AudioDeviceEventKind Option { get; set; }

        public override string Describe() =>string.Format(Properties.Resources.DeviceEventTriggerDescribeFormatText, Device, Options[0].DisplayName);

        public DeviceEventTrigger()
        {
            Description = Properties.Resources.DeviceEventTriggerDescriptionText;
            Options = new List<OptionCollection>(new OptionCollection[]{ new OptionCollection(new List<Option>
                {
                    new Option(Properties.Resources.AudioDeviceEventKindAddedText, AudioDeviceEventKind.Added),
                    new Option(Properties.Resources.AudioDeviceEventKindRemovedText, AudioDeviceEventKind.Removed),
                    new Option(Properties.Resources.AudioDeviceEventKindBecomesDefaultText, AudioDeviceEventKind.BecomingDefault),
                    new Option(Properties.Resources.AudioDeviceEventKindLeavesDefaultText, AudioDeviceEventKind.LeavingDefault),
                },
                (newValue) => Option = (AudioDeviceEventKind)newValue.Value,
                () => Option) });
        }
    }
}