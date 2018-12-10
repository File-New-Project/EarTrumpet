using EarTrumpet_Actions.DataModel.Enum;
using System.Collections.Generic;

namespace EarTrumpet_Actions.DataModel.Actions
{
    public class SetVariableAction : BaseAction, IPartWithText
    {
        public string Text { get; set; }
        public BoolValue Value { get; set; }

        public string PromptText => Properties.Resources.SetVariableActionPromptText;
        public string EmptyText => Properties.Resources.VariableConditionEmptyText;
    }
}
