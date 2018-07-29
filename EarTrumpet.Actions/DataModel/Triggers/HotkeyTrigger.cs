using EarTrumpet.Interop.Helpers;

namespace EarTrumpet_Actions.DataModel.Triggers
{
    public class HotkeyTrigger : BaseTrigger
    {
        public HotkeyData Hotkey { get; set; }

        public HotkeyTrigger()
        {
            Description = "When a hotkey is pressed";
            Hotkey = new HotkeyData();
        }

        public override string Describe() => $"When {Hotkey} is pressed";
    }
}