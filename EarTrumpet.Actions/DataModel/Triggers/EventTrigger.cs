using EarTrumpet_Actions.DataModel.Enum;
using System.Collections.Generic;

namespace EarTrumpet_Actions.DataModel.Triggers
{
    public class EventTrigger : BaseTrigger
    {
        public EarTrumpetEventKind Option { get; set; }

        public EventTrigger()
        {
            Description = Properties.Resources.EventTriggerDescriptionText;
        }

        public override string Describe() => Properties.Resources.EventTriggerDescribeFormatText;
    }
}
