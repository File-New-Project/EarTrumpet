using EarTrumpet.DataModel;
using EarTrumpet.Services;

namespace EarTrumpet.ViewModels
{
    public class SettingsViewModel : BindableBase
    {
        IAudioDeviceManager _manager;

        SettingsService.HotkeyData _hotkey;
        public SettingsService.HotkeyData Hotkey
        {
            get => _hotkey;
            set
            {
                _hotkey = value;
                RaisePropertyChanged(nameof(Hotkey));
                RaisePropertyChanged(nameof(HotkeyText));
            }
        }

        public string HotkeyText => _hotkey.ToString();

        public SettingsViewModel(IAudioDeviceManager manager)
        {
            _manager = manager;

            Hotkey = SettingsService.Hotkey;
        }

        public void Save()
        {
            SettingsService.Hotkey = Hotkey;
            HotkeyService.Register(Hotkey.Modifiers, Hotkey.Key);
        }
    }
}
