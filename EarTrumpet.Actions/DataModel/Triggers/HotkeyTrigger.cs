namespace EarTrumpet_Actions.DataModel.Triggers
{
    public class HotkeyTrigger : BaseTrigger
    {
        public EarTrumpet.UI.Services.HotkeyData Hotkey { get; set; }

        public HotkeyTrigger()
        {
            Description = "When a hotkey is pressed";
            Hotkey = new EarTrumpet.UI.Services.HotkeyData();
        }

        public override string Describe() => $"When {Hotkey} is pressed";
    }
}