using EarTrumpet_Actions.DataModel.Conditions;
using System.Collections.Generic;
using System.Linq;

namespace EarTrumpet_Actions.DataModel.Triggers
{
    public class ProcessTrigger : BaseTrigger
    {
        public string ProcessName { get; set; }
        public ProcessConditionType ConditionType { get; set; }

        public ProcessTrigger()
        {
            DisplayName = "When a process starts or stops";
            Options = new List<Option>
            {
                new Option("starts", ProcessConditionType.IsRunning),
                new Option("stops", ProcessConditionType.IsNotRunning),
            };
            ActionsManager.Instance.ProcessWatcher.ProcessStarted += ProcessWatcher_ProcessStarted;
            ActionsManager.Instance.ProcessWatcher.ProcessStopped += ProcessWatcher_ProcessStopped;
        }

        private void ProcessWatcher_ProcessStopped(string obj)
        {
            if (ConditionType == ProcessConditionType.IsNotRunning && obj == ProcessName)
            {
                OnTriggered();
            }
        }

        private void ProcessWatcher_ProcessStarted(string obj)
        {
            if (ConditionType == ProcessConditionType.IsRunning && obj == ProcessName)
            {
                OnTriggered();
            }
        }

        public override void Close()
        {

        }

        public override void Loaded()
        {
            var selected = Options.First(o => (ProcessConditionType)o.Value == ConditionType);
            Option = selected.Value;


            if (ActionsManager.Instance.ProcessWatcher.ProcessNames.Contains(ProcessName))
            {
                ProcessWatcher_ProcessStarted(ProcessName);
            }
            else
            {
                ProcessWatcher_ProcessStopped(ProcessName);
            }

            DisplayName = $"{ProcessName} {selected.DisplayName}";
        }
    }
}