using EarTrumpet.UI.Helpers;
using EarTrumpet_Actions.DataModel.Triggers;
using System.Windows.Input;

namespace EarTrumpet_Actions.ViewModel.Triggers
{
    class HotkeyTriggerViewModel : PartViewModel
    {
        public string Hotkey => _trigger.Hotkey.IsEmpty ? "Choose a key" : _trigger.Hotkey.ToString();

        public ICommand SetHotkey { get; }

        private HotkeyTrigger _trigger;

        public HotkeyTriggerViewModel(ActionsEditorViewModel parent, HotkeyTrigger trigger) : base(trigger)
        {
            _trigger = trigger;

            SetHotkey = new RelayCommand(() =>
            {
                _trigger.Hotkey = parent.GetHotkey(_trigger.Hotkey);
                RaisePropertyChanged(nameof(Hotkey));
                UpdateDescription();
            });
        }
    }
}
