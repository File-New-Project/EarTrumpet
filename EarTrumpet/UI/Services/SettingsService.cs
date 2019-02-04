using EarTrumpet.DataModel.Storage;
using EarTrumpet.Interop.Helpers;
using System;
using System.Windows.Forms;

namespace EarTrumpet.UI.Services
{
    public class SettingsService
    {
        public static event EventHandler<bool> UseLegacyIconChanged;

        public static readonly HotkeyData s_defaultHotkey = new HotkeyData { Modifiers = Keys.Shift | Keys.Control, Key = Keys.Q };
        public static readonly HotkeyData s_defaultMixerHotkey = new HotkeyData { Modifiers = Keys.Shift | Keys.Control, Key = Keys.W };

        private static ISettingsBag s_settings = StorageFactory.GetSettings();

        public static HotkeyData FlyoutHotkey
        {
            get => s_settings.Get("Hotkey", s_defaultHotkey);
            set => s_settings.Set("Hotkey", value);
        }

        public static HotkeyData MixerHotkey
        {
            get => s_settings.Get("MixerHotkey", s_defaultMixerHotkey);
            set => s_settings.Set("MixerHotkey", value);
        }

        public static bool UseLegacyIcon
        {
            get
            {
                // Note: Legacy compat, we used to write string bools.
                var ret = s_settings.Get("UseLegacyIcon", "False");
                bool.TryParse(ret, out bool isUseLegacyIcon);
                return isUseLegacyIcon;
            }
            set
            {
                s_settings.Set("UseLegacyIcon", value.ToString());
                UseLegacyIconChanged?.Invoke(null, UseLegacyIcon);
            }
        }
    }
}