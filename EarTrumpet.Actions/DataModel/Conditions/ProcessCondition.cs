using EarTrumpet_Actions.DataModel.Enum;
using System.Collections.Generic;

namespace EarTrumpet_Actions.DataModel.Conditions
{
    public class ProcessCondition : BaseCondition, IPartWithText
    {
        public string PromptText => Properties.Resources.ProcessConditionPromptText;
        public string EmptyText => Properties.Resources.ProcessConditionEmptyText;
        public string Text { get; set; }

        public ProcessStateKind Option { get; set; }
    }
}
