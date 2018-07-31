using System.Collections.Generic;

namespace EarTrumpet_Actions.DataModel.Actions
{
    public class SetVariableAction : BaseAction, IPartWithText
    {
        public string Text { get; set; }
        public bool Value { get; set; }

        public string PromptText => Properties.Resources.SetVariableActionPromptText;

        public override string Describe() => $"Set variable '{Text}' to {Options[0].DisplayName}";

        public SetVariableAction()
        {
            Description = Properties.Resources.SetVariableActionDescriptionText;
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
