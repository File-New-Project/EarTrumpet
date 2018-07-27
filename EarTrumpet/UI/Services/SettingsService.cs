using EarTrumpet.DataModel.Storage;
using System;
using System.Windows.Forms;

namespace EarTrumpet.UI.Services
{
    public class SettingsService
    {
        public static event EventHandler<bool> UseLegacyIconChanged;

        public static readonly HotkeyData s_defaultHotkey = new HotkeyData { Modifiers = Keys.Shift | Keys.Control, Key = System.Windows.Forms.Keys.Q };

        private static ISettingsBag s_settings = StorageFactory.GetSettings();

        public static HotkeyData Hotkey
        {
            get => s_settings.Get("Hotkey", s_defaultHotkey);
            set => s_settings.Set("Hotkey", value);
        }

        public static bool UseLegacyIcon
        {
            get
            {
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