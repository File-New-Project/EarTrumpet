using EarTrumpet.UI.Services;

namespace EarTrumpet.UI.ViewModels
{
    internal class EarTrumpetShortcutsPageViewModel : SettingsPageViewModel
    {
        public HotkeyViewModel OpenFlyoutHotkey { get; }
        public string DefaultHotKey => SettingsService.s_defaultFlyoutHotkey.ToString();

        public HotkeyViewModel OpenMixerHotkey { get; }
        public string DefaultMixerHotKey => SettingsService.s_defaultMixerHotkey.ToString();

        public HotkeyViewModel OpenSettingsHotkey { get; }
        public string DefaultSettingsHotKey => SettingsService.s_defaultSettingsHotkey.ToString();

        public EarTrumpetShortcutsPageViewModel() : base(null)
        {
            Title = Properties.Resources.ShortcutsPageText;
            Glyph = "\xE765";

            OpenFlyoutHotkey = new HotkeyViewModel(SettingsService.FlyoutHotkey, (newHotkey) => SettingsService.FlyoutHotkey = newHotkey);
            OpenMixerHotkey = new HotkeyViewModel(SettingsService.MixerHotkey, (newHotkey) => SettingsService.MixerHotkey = newHotkey);
            OpenSettingsHotkey = new HotkeyViewModel(SettingsService.SettingsHotkey, (newHotkey) => SettingsService.SettingsHotkey = newHotkey);
        }
    }
}