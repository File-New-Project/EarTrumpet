using EarTrumpet.Interop.Helpers;

namespace EarTrumpet_Actions.DataModel.Triggers
{
    public class HotkeyTrigger : BaseTrigger
    {
        public HotkeyData Option { get; set; } = new HotkeyData();
    }
}