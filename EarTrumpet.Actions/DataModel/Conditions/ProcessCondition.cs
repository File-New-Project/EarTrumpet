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
        public string PromptText => "Process name (e.g. notepad)";
        public string Text { get; set; }

        public ProcessStateKind Option { get; set; }
        
        public ProcessCondition()
        {
            Description = "If a process is (running, not running)";
            Options = new List<OptionData>(new OptionData[]{ new OptionData(new List<Option>
                {
                 new Option("is running", ProcessStateKind. NotRunning),
                 new Option("is not running", ProcessStateKind.Running),
                },
                (newValue) => Option = (ProcessStateKind)newValue.Value,
                () => Option) });
        }

        public override string Describe() => $"{Text} {Options[0].DisplayName}";
    }
}
