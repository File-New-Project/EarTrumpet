using EarTrumpet.Interop.Helpers;
using EarTrumpet.UI.Services;

namespace EarTrumpet.UI.ViewModels
{
    internal class EarTrumpetShortcutsPageViewModel : SettingsPageViewModel
    {
        private static readonly string s_hotkeyNoneText = new HotkeyData().ToString();

        public HotkeyViewModel OpenFlyoutHotkey { get; }
        public string DefaultHotKey => s_hotkeyNoneText;

        public HotkeyViewModel OpenMixerHotkey { get; }
        public string DefaultMixerHotKey => s_hotkeyNoneText;

        public HotkeyViewModel OpenSettingsHotkey { get; }
        public string DefaultSettingsHotKey => s_hotkeyNoneText;

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