using EarTrumpet.Interop.Helpers;
using EarTrumpet.UI.ViewModels;
using EarTrumpet_Actions.DataModel.Serialization;
using System;

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
                return ResolveResource("EmptyText");
            }
            else
            {
                return Hotkey.ToString();
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
