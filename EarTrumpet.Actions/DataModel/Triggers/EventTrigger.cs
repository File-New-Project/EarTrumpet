using System;
using System.Collections.Generic;
using System.Linq;

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
            OnEvent += (t) => OnTriggered();
            DisplayName = "When EarTrumpet is started or stopped";
            Options = new List<Option>
            {
                new Option("starts up", EventTriggerType.EarTrumpet_Startup),
                new Option("is shutting down", EventTriggerType.EarTrumpet_Shutdown),
            };
        }

        public override void Close()
        {

        }

        public override void Loaded()
        {
            var selected = Options.First(o => (EventTriggerType)o.Value == TriggerType);
            Option = selected.Value;
            DisplayName = $"When EarTrumpet {selected}";
        }
    }
}
