using System.Collections.Generic;

namespace EarTrumpet_Actions.DataModel.Conditions
{
    public class VariableCondition : BaseCondition, IPartWithText
    {
        public string Text { get; set; }
        public bool Value { get; set; }

        public string PromptText => Properties.Resources.VariableConditionPromptText;

        public override string Describe() => string.Format(Properties.Resources.VariableConditionDescribeFormatText, Text, Options[0].DisplayName);

        public VariableCondition()
        {
            Description = Properties.Resources.VariableConditionDescriptionText;
            Options = new List<OptionData>(new OptionData[]{ new OptionData(new List<Option>
                {
                     new Option(Properties.Resources.BoolTrueText, true),
                     new Option(Properties.Resources.BoolFalseText, false),
                },
                (newValue) => Value = (bool)newValue.Value,
                () => Value) });
        }
    }
}
