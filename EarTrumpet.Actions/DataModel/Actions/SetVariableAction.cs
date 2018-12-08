using System.Collections.Generic;

namespace EarTrumpet_Actions.DataModel.Actions
{
    public class SetVariableAction : BaseAction, IPartWithText
    {
        public string Text { get; set; }
        public bool Value { get; set; }

        public string PromptText => Properties.Resources.SetVariableActionPromptText;

        public override string Describe() => Properties.Resources.SetVariableActionDescribeFormatText;

        public SetVariableAction()
        {
            Description = Properties.Resources.SetVariableActionDescriptionText;
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
