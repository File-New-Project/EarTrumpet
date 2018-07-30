using EarTrumpet.Interop.Helpers;
using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.Services;
using EarTrumpet_Actions.DataModel.Triggers;
using System.Windows.Input;

namespace EarTrumpet_Actions.ViewModel.Triggers
{
    class HotkeyTriggerViewModel : PartViewModel
    {
        public string HotkeyText => _trigger.Option.IsEmpty ? "Choose a key" : _trigger.Option.ToString();

        public HotkeyData Hotkey
        {
            get => _trigger.Option;
            set
            {
                if (Hotkey != value)
                {
                    _trigger.Option = value;
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
