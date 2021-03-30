using EarTrumpet.Actions.DataModel.Serialization;
using System;

namespace EarTrumpet.Actions.ViewModel
{
    public class HotkeyViewModel : BindableBase
    {
        public EarTrumpet.UI.ViewModels.HotkeyViewModel Hotkey { get; }

        private HotkeyTrigger _trigger;

        public HotkeyViewModel(HotkeyTrigger trigger)
        {
            _trigger = trigger;
            Hotkey = new EarTrumpet.UI.ViewModels.HotkeyViewModel(_trigger.Option, (newHotkey) =>
            {
                _trigger.Option = newHotkey;
                RaisePropertyChanged(nameof(Hotkey));
            });
        }

        public override string ToString()
        {
            if (_trigger.Option.IsEmpty)
            {
                return ResolveResource("EmptyText");
            }
            else
            {
                return _trigger.Option.ToString();
            }
        }

        private string ResolveResource(string suffix)
        {
            var res = $"{_trigger.GetType().Name}_{suffix}";
            var ret = Properties.Resources.ResourceManager.GetString(res);
            if (string.IsNullOrWhiteSpace(ret))
            {
                throw new NotImplementedException($"Missing resource: {res}");
            }
            return ret;
        }
    }
}
