using EarTrumpet_Actions.DataModel.Enum;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace EarTrumpet_Actions.DataModel.Triggers
{
    public class ProcessTrigger : BaseTrigger, IPartWithText
    {
        public string Text { get; set; }

        [XmlIgnore]
        public string PromptText => Properties.Resources.ProcessTriggerDescriptonPromptText;
        public string EmptyText => Properties.Resources.ProcessConditionEmptyText;
        public ProcessEventKind Option { get; set; }
    }
}