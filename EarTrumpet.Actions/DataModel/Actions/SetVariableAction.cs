using System.Collections.Generic;

namespace EarTrumpet_Actions.DataModel.Actions
{
    public class SetVariableAction : BaseAction, IPartWithText
    {
        public string Text { get; set; }
        public bool Value { get; set; }

        public string PromptText => "Enter a variable name (e.g. IsInGameMode)";

        public override string Describe() => $"Set variable '{Text}' to {Options[0].DisplayName}";

        public SetVariableAction()
        {
            Description = "Set a local variable to True or False";
            Options = new List<OptionData>(new OptionData[]{ new OptionData(new List<Option>
                {
                     new Option("True", true),
                     new Option("False", false),
                },
                (newValue) => Value = (bool)newValue.Value,
                () => Value) });
        }
    }
}
