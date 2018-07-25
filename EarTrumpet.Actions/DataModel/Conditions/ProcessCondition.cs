using System;
using System.Collections.Generic;

namespace EarTrumpet_Actions.DataModel.Conditions
{
    public enum ProcessConditionType
    {
        IsRunning,
        IsNotRunning
    }

    public class ProcessCondition : BaseCondition, IPartWithText
    {
        public string PromptText => "Process name (e.g. notepad)";
        public string Text { get; set; }

        public ProcessConditionType ConditionType { get; set; }
        
        public ProcessCondition()
        {
            Description = "If a process is (running, not running)";
            Options = new List<OptionData>(new OptionData[]{ new OptionData(new List<Option>
                {
                 new Option("is running", ProcessConditionType. IsNotRunning),
                 new Option("is not running", ProcessConditionType.IsRunning),
                },
                (newValue) => ConditionType = (ProcessConditionType)newValue.Value,
                () => ConditionType) });
        }

        public override string Describe() => $"{Text} {Options[0].DisplayName}";
    }
}
