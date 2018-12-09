using EarTrumpet.Interop.Helpers;
using EarTrumpet.UI.ViewModels;
using EarTrumpet_Actions.DataModel.Triggers;

namespace EarTrumpet_Actions.ViewModel
{
    public class HotkeyViewModel : BindableBase
    {
        public HotkeyData Hotkey
        {
            get => _trigger.Option;
            set
            {
                if (Hotkey != value)
                {
                    _trigger.Option = value;
                    RaisePropertyChanged(nameof(Hotkey));
                }
            }
        }

        private HotkeyTrigger _trigger;

        public HotkeyViewModel(HotkeyTrigger trigger)
        {
            _trigger = trigger;
        }

        public override string ToString()
        {
            if (Hotkey.IsEmpty)
            {
                // TODO loc
                return "(choose a hotkey)";
            }
            else
            {
                return Hotkey.ToString();
            }
        }
    }
}
