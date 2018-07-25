using EarTrumpet_Actions.DataModel.Triggers;

namespace EarTrumpet_Actions.ViewModel.Triggers
{
    class HotkeyTriggerViewModel : PartViewModel
    {
      //  public string Hotkey => _trigger.

        private HotkeyTrigger _trigger;
             
        public HotkeyTriggerViewModel(HotkeyTrigger trigger) : base(trigger)
        {
            _trigger = trigger;
        }
    }
}
