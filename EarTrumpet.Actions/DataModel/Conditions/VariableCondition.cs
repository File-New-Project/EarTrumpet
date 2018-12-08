using System.Collections.Generic;

namespace EarTrumpet_Actions.DataModel.Conditions
{
    public class VariableCondition : BaseCondition, IPartWithText
    {
        public string Text { get; set; }
        public bool Value { get; set; }

        public string PromptText => Properties.Resources.VariableConditionPromptText;
        public string EmptyText => Properties.Resources.VariableConditionEmptyText;

        public override string Describe() => Properties.Resources.VariableConditionDescribeFormatText;

        public VariableCondition()
        {
            Description = Properties.Resources.VariableConditionDescriptionText;
            Options = new List<OptionCollection>(new OptionCollection[]{ new OptionCollection(new List<Option>
                {
                     new Option(Properties.Resources.BoolTrueText, true),
                     new Option(Properties.Resources.BoolFalseText, false),
                },
                (newValue) => Value = (bool)newValue.Value,
                () => Value) });
        }
    }
}
