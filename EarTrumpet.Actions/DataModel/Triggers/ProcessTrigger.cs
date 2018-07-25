using System.Collections.Generic;
using System.Xml.Serialization;

namespace EarTrumpet_Actions.DataModel.Triggers
{
    public enum ProcessTriggerConditionType
    {
        Starts,
        Stops,
    }

    public class ProcessTrigger : BaseTrigger, IPartWithText
    {
        public string Text { get; set; }

        [XmlIgnore]
        public string PromptText { get; private set; }
        public ProcessTriggerConditionType ConditionType { get; set; }

        public ProcessTrigger()
        {
            PromptText = "Process name (e.g. notepad)";

            Description = "When a process starts or stops";
            Options = new List<OptionData>(new OptionData[]{ new OptionData(new List<Option>
                {
                    new Option("starts", ProcessTriggerConditionType.Starts),
                    new Option("stops", ProcessTriggerConditionType.Stops),
                },
                (newValue) => ConditionType = (ProcessTriggerConditionType)newValue.Value,
                () => ConditionType) });
        }

        public override string Describe() => $"{Text} {Options[0].DisplayName}";
    }
}