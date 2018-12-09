using EarTrumpet_Actions.DataModel.Enum;
using System.Collections.Generic;

namespace EarTrumpet_Actions.DataModel.Triggers
{
    public class DeviceEventTrigger : BaseTrigger, IPartWithDevice
    {
        public Device Device { get; set; }
        public AudioDeviceEventKind Option { get; set; }

        public override string Describe() => Properties.Resources.DeviceEventTriggerDescribeFormatText;

        public DeviceEventTrigger()
        {
            Description = Properties.Resources.DeviceEventTriggerDescriptionText;
        }
    }
}