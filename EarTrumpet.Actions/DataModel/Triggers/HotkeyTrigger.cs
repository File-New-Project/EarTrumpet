namespace EarTrumpet_Actions.DataModel.Triggers
{
    public class HotkeyTrigger : BaseTrigger
    {
        public EarTrumpet.UI.Services.HotkeyData Hotkey { get; set; }

        public HotkeyTrigger()
        {
            DisplayName = "When a hotkey is pressed";
        }

        public override void Close()
        {

        }

        public override void Loaded()
        {
            DisplayName = $"When {Hotkey} is pressed";
        }
    }
}