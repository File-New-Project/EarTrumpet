using EarTrumpet_Actions.DataModel.Conditions;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace EarTrumpet_Actions.DataModel.Triggers
{
    public class ProcessTrigger : BaseTrigger, IPartWithText
    {
        public string Text { get; set; }

        [XmlIgnore]
        public string PromptText { get; private set; }
        public ProcessConditionType ConditionType { get; set; }

        public ProcessTrigger()
        {
            PromptText = "Process name (e.g. notepad)";

            Description = "When a process starts or stops";
            Options = new List<OptionData>(new OptionData[]{ new OptionData(new List<Option>
                {
                    new Option("starts", ProcessConditionType.IsRunning),
                    new Option("stops", ProcessConditionType.IsNotRunning),
                },
                (newValue) => ConditionType = (ProcessConditionType)newValue.Value,
                () => ConditionType) });

            Addon.Current.Manager.ProcessWatcher.ProcessStarted += ProcessWatcher_ProcessStarted;
            Addon.Current.Manager.ProcessWatcher.ProcessStopped += ProcessWatcher_ProcessStopped;
        }

        private void ProcessWatcher_ProcessStopped(string processName)
        {
            if (ConditionType == ProcessConditionType.IsNotRunning && processName == Text)
            {
                RaiseTriggered();
            }
        }

        private void ProcessWatcher_ProcessStarted(string processName)
        {
            if (ConditionType == ProcessConditionType.IsRunning && processName == Text)
            {
                RaiseTriggered();
            }
        }

        public override void Loaded()
        {
            if (Addon.Current.Manager.ProcessWatcher.ProcessNames.Contains(Text))
            {
                ProcessWatcher_ProcessStarted(Text);
            }
            else
            {
                ProcessWatcher_ProcessStopped(Text);
            }
        }

        public override string Describe() => $"{Text} {Options[0].DisplayName}";
    }
}