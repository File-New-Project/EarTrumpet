using System;
using System.Collections.Generic;

namespace EarTrumpet_Actions.DataModel.Triggers
{
    public enum EventTriggerType
    {
        EarTrumpet_Startup,
        EarTrumpet_Shutdown,
    };

    public class EventTrigger : BaseTrigger
    {
        private static event Action<EventTriggerType> OnEvent;

        public void RaiseEvent(EventTriggerType et) => OnEvent?.Invoke(et);

        public EventTriggerType TriggerType { get; set; }

        public EventTrigger()
        {
            OnEvent += (t) => RaiseTriggered();


            Description = "When EarTrumpet is started or stopped";
            Options = new List<OptionData>(new OptionData[]{ new OptionData(new List<Option>
                {
                    new Option("starts up", EventTriggerType.EarTrumpet_Startup),
                    new Option("is shutting down", EventTriggerType.EarTrumpet_Shutdown),
                },
                (newValue) => TriggerType = (EventTriggerType)newValue.Value,
                () => TriggerType) });
        }

        public override string Describe() => $"When EarTrumpet {Options[0].DisplayName}";
    }
}
