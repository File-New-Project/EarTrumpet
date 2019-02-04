using EarTrumpet.UI.Services;

namespace EarTrumpet.UI.ViewModels
{
    internal class EarTrumpetShortcutsPageViewModel : SettingsPageViewModel
    {
        public HotkeyViewModel OpenFlyoutHotkey { get; }
        public string DefaultHotKey => SettingsService.s_defaultHotkey.ToString();

        public EarTrumpetShortcutsPageViewModel() : base(null)
        {
            Title = Properties.Resources.ShortcutsPageText;
            Glyph = "\xE765";

            OpenFlyoutHotkey = new HotkeyViewModel(SettingsService.Hotkey, (newHotkey) => SettingsService.Hotkey = newHotkey);
        }
    }
}