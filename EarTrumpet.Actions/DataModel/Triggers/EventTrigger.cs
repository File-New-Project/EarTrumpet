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
            Options = new List<OptionData>(new OptionData[]{ new OptionData(new List<Option>
                {
                    new Option(Properties.Resources.EarTrumpetEventKindStartupText, EarTrumpetEventKind.Startup),
                    new Option(Properties.Resources.EarTrumpetEventKindShutdownText, EarTrumpetEventKind.Shutdown),
                },
                (newValue) => Option = (EarTrumpetEventKind)newValue.Value,
                () => Option) });
        }

        public override string Describe() => $"EarTrumpet {Options[0].DisplayName}";
    }
}
