using EarTrumpet.Interop.Helpers;
using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.Services;
using EarTrumpet_Actions.DataModel.Triggers;
using System.Windows.Input;

namespace EarTrumpet_Actions.ViewModel.Triggers
{
    class HotkeyTriggerViewModel : PartViewModel
    {
        public string HotkeyText => _trigger.Hotkey.IsEmpty ? "Choose a key" : _trigger.Hotkey.ToString();

        public HotkeyData Hotkey
        {
            get => _trigger.Hotkey;
            set
            {
                if (Hotkey != value)
                {
                    _trigger.Hotkey = value;
                    RaisePropertyChanged(nameof(Hotkey));
                    RaisePropertyChanged(nameof(HotkeyText));
                    UpdateDescription();
                }
            }
        }

        public ICommand SetHotkey { get; set; }

        private HotkeyTrigger _trigger;

        public HotkeyTriggerViewModel(HotkeyTrigger trigger) : base(trigger)
        {
            _trigger = trigger;
        }
    }
}
