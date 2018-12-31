using EarTrumpet.DataModel.Storage;
using EarTrumpet.Interop.Helpers;
using System;
using System.Windows.Forms;

namespace EarTrumpet.UI.Services
{
    public class SettingsService
    {
        #region Add Me
        //Startup
        public static event EventHandler<bool> StartupChanged;
        public static bool Startup
        {
            get
            {
                var ret = StartupWindows.GetStartup();// S_settings.Get("Startup", false);
                return ret;
            }
            set
            {
                StartupWindows.SetStartup(value);
                StartupChanged?.Invoke(null, Startup);
            }
        }
        //Language
        public static event EventHandler<string> LanguageChanged;
        public static string Language
        {
            get
            {
                var ret = S_settings.Get("Language", "Auto");
                if (ret == null)
                    ret = "Auto";
                return ret;
            }
            set
            {
                S_settings.Set("Language", value);
                App._Language.SwitchLanguage(value);
                LanguageChanged?.Invoke(null, Language);
            }
        }
        public static bool Welcome
        {
            get
            {
                var ret = S_settings.Get("Welcome", true);
                return ret;
            }
            set
            {
                S_settings.Set("Welcome", value);
            }
        }
        #endregion
        //
        public static event EventHandler<bool> UseLegacyIconChanged;

        public static readonly HotkeyData s_defaultHotkey = new HotkeyData { Modifiers = Keys.Shift | Keys.Control, Key = Keys.Q };

        private static readonly ISettingsBag s_settings = StorageFactory.GetSettings();

        public static HotkeyData Hotkey
        {
            get => S_settings.Get("Hotkey", s_defaultHotkey);
            set => S_settings.Set("Hotkey", value);
        }

        public static bool UseLegacyIcon
        {
            get
            {
                var ret = S_settings.Get("UseLegacyIcon", "False");
                bool.TryParse(ret, out bool isUseLegacyIcon);
                return isUseLegacyIcon;
            }
            set
            {
                S_settings.Set("UseLegacyIcon", value.ToString());
                UseLegacyIconChanged?.Invoke(null, UseLegacyIcon);
            }
        }

        public static ISettingsBag S_settings => s_settings;
    }
}