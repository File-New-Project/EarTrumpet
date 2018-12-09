using EarTrumpet_Actions.DataModel.Enum;
using System.Collections.Generic;

namespace EarTrumpet_Actions.DataModel.Triggers
{
    public class AppEventTrigger : BaseTrigger, IPartWithDevice, IPartWithApp
    {
        public Device Device { get; set; }
        public App App { get; set; }
        public AudioAppEventKind Option { get; set; }
        
        public AppEventTrigger()
        {
            Description = Properties.Resources.AppEventTriggerDescriptionText;
        }

        public override string Describe() => Properties.Resources.AppEventTriggerDescribeFormatText;
    }
}
