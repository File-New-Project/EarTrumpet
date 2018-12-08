using EarTrumpet.Interop.Helpers;

namespace EarTrumpet_Actions.DataModel.Triggers
{
    public class HotkeyTrigger : BaseTrigger
    {
        public HotkeyData Option { get; set; }

        public HotkeyTrigger()
        {
            Description = Properties.Resources.HotkeyTriggerDescriptionText;
            Option = new HotkeyData();
        }

        public override string Describe() => Properties.Resources.HotkeyTriggerDescribeFormatText;
    }
}