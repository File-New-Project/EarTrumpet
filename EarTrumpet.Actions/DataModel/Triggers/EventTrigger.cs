using System;
using System.Collections.Generic;

namespace EarTrumpet_Actions.DataModel.Triggers
{
    public enum EarTrumpetEventKind
    {
        Startup,
        Shutdown,
    };

    public class EventTrigger : BaseTrigger
    {
        public EarTrumpetEventKind Option { get; set; }

        public EventTrigger()
        {
            Description = "When EarTrumpet is started or stopped";
            Options = new List<OptionData>(new OptionData[]{ new OptionData(new List<Option>
                {
                    new Option("starts up", EarTrumpetEventKind.Startup),
                    new Option("is shutting down", EarTrumpetEventKind.Shutdown),
                },
                (newValue) => Option = (EarTrumpetEventKind)newValue.Value,
                () => Option) });
        }

        public override string Describe() => $"When EarTrumpet {Options[0].DisplayName}";
    }
}
