using System;
using System.Collections.Generic;

namespace EarTrumpet_Actions.DataModel.Conditions
{
    public enum ProcessStateKind
    {
        Running,
        NotRunning
    }

    public class ProcessCondition : BaseCondition, IPartWithText
    {
        public string PromptText => Properties.Resources.ProcessConditionPromptText;
        public string Text { get; set; }

        public ProcessStateKind Option { get; set; }
        
        public ProcessCondition()
        {
            Description = Properties.Resources.ProcessConditionDescriptionText;
            Options = new List<OptionData>(new OptionData[]{ new OptionData(new List<Option>
                {
                 new Option(Properties.Resources.ProcessStateKindNotRunningText, ProcessStateKind. NotRunning),
                 new Option(Properties.Resources.ProcessStateKindRunningText, ProcessStateKind.Running),
                },
                (newValue) => Option = (ProcessStateKind)newValue.Value,
                () => Option) });
        }

        public override string Describe() => $"{Text} {Options[0].DisplayName}";
    }
}
