using EarTrumpet_Actions.DataModel.Enum;
using System.Collections.Generic;

namespace EarTrumpet_Actions.DataModel.Conditions
{
    public class VariableCondition : BaseCondition, IPartWithText
    {
        public string Text { get; set; }
        public BoolValue Value { get; set; }

        public string PromptText => Properties.Resources.VariableConditionPromptText;
        public string EmptyText => Properties.Resources.VariableConditionEmptyText;
    }
}
