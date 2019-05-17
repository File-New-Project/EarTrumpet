using EarTrumpet.Interop.Helpers;

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

        public EarTrumpetShortcutsPageViewModel(AppSettings settings) : base(null)
        {
            Title = Properties.Resources.ShortcutsPageText;
            Glyph = "\xE765";

            OpenFlyoutHotkey = new HotkeyViewModel(settings.FlyoutHotkey, (newHotkey) => settings.FlyoutHotkey = newHotkey);
            OpenMixerHotkey = new HotkeyViewModel(settings.MixerHotkey, (newHotkey) => settings.MixerHotkey = newHotkey);
            OpenSettingsHotkey = new HotkeyViewModel(settings.SettingsHotkey, (newHotkey) => settings.SettingsHotkey = newHotkey);
        }
    }
}