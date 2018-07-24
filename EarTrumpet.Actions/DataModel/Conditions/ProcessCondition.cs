using System;
using System.Collections.Generic;
using System.Linq;

namespace EarTrumpet_Actions.DataModel.Conditions
{
    public enum ProcessConditionType
    {
        IsRunning,
        IsNotRunning
    }

    public class ProcessCondition : BaseCondition
    {
        public string ProcessName { get; set; }

        public ProcessConditionType ConditionType { get; set; }
        
        public ProcessCondition()
        {
            DisplayName = "If a process is (running, not running)";
            Options = new List<Option>
            {
                new Option("is running", ProcessConditionType. IsNotRunning),
                new Option("is not running", ProcessConditionType.IsRunning),
            };
        }

        public override void Loaded()
        {
            var selected = Options.First(o => (ProcessConditionType)o.Value == ConditionType);
            Option = selected.Value;


            DisplayName = $"{ProcessName} {Options}";
        }

        public override bool IsMet()
        {
            bool ret = ActionsManager.Instance.ProcessWatcher.ProcessNames.Contains(ProcessName);
            
            switch(ConditionType)
            {
                case ProcessConditionType.IsRunning:
                    return ret;
                case ProcessConditionType.IsNotRunning:
                    return !ret;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
