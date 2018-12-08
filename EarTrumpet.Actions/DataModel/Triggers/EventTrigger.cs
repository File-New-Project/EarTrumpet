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
            Options = new List<OptionCollection>(new OptionCollection[]{ new OptionCollection(new List<Option>
                {
                    new Option(Properties.Resources.EarTrumpetEventKindStartupText, EarTrumpetEventKind.Startup),
                    new Option(Properties.Resources.EarTrumpetEventKindShutdownText, EarTrumpetEventKind.Shutdown),
                },
                (newValue) => Option = (EarTrumpetEventKind)newValue.Value,
                () => Option) });
        }

        public override string Describe() => string.Format(Properties.Resources.EventTriggerDescribeFormatText, Options[0].DisplayName);
    }
}
