using EarTrumpet.Interop.Helpers;

namespace EarTrumpet_Actions.DataModel.Triggers
{
    public class HotkeyTrigger : BaseTrigger
    {
        public HotkeyData Option { get; set; }

        public HotkeyTrigger()
        {
            Description = "When a hotkey is pressed";
            Option = new HotkeyData();
        }

        public override string Describe() => $"When {Option} is pressed";
    }
}