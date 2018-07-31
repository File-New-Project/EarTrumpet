using System.Collections.Generic;
using System.Xml.Serialization;

namespace EarTrumpet_Actions.DataModel.Triggers
{
    public enum ProcessEventKind
    {
        Start,
        Stop,
    }

    public class ProcessTrigger : BaseTrigger, IPartWithText
    {
        public string Text { get; set; }

        [XmlIgnore]
        public string PromptText { get; private set; }
        public ProcessEventKind Option { get; set; }

        public ProcessTrigger()
        {
            PromptText = Properties.Resources.ProcessTriggerDescriptonPromptText;
            Description = Properties.Resources.ProcessTriggerDescriptonText;
            Options = new List<OptionData>(new OptionData[]{ new OptionData(new List<Option>
                {
                    new Option(Properties.Resources.ProcessEventKindStartsText, ProcessEventKind.Start),
                    new Option(Properties.Resources.ProcessEventKindStopsText, ProcessEventKind.Stop),
                },
                (newValue) => Option = (ProcessEventKind)newValue.Value,
                () => Option) });
        }

        public override string Describe() => $"{Text} {Options[0].DisplayName}";
    }
}