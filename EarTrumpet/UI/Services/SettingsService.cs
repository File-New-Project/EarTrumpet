using EarTrumpet.DataModel.Storage;
using EarTrumpet.Interop.Helpers;
using System;

namespace EarTrumpet.UI.Services
{
    public class SettingsService
    {
        public static event EventHandler<bool> UseLegacyIconChanged;
        public static event EventHandler<bool> StartupChanged;
        public static event EventHandler<string> LanguageChanged;
        public static readonly HotkeyData s_defaultFlyoutHotkey = new HotkeyData { };
        public static readonly HotkeyData s_defaultMixerHotkey = new HotkeyData { };
        public static readonly HotkeyData s_defaultSettingsHotkey = new HotkeyData { };

        private static ISettingsBag s_settings = StorageFactory.GetSettings();

        public static HotkeyData FlyoutHotkey
        {
            get => s_settings.Get("Hotkey", s_defaultFlyoutHotkey);
            set
            {
                s_settings.Set("Hotkey", value);
                HotkeyManager.Current.Register(FlyoutHotkey);
            }
        }

        public static HotkeyData MixerHotkey
        {
            get => s_settings.Get("MixerHotkey", s_defaultMixerHotkey);
            set
            {
                s_settings.Set("MixerHotkey", value);
                HotkeyManager.Current.Register(MixerHotkey);
            }
        }

        public static HotkeyData SettingsHotkey
        {
            get => s_settings.Get("SettingsHotkey", s_defaultSettingsHotkey);
            set
            {
                s_settings.Set("SettingsHotkey", value);
                HotkeyManager.Current.Register(SettingsHotkey);
            }
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
        //hamed
        public static bool Startup
        {
            get
            {
                // Note: Legacy compat, we used to write string bools.
                var ret = s_settings.Get("Startup", "False");
                bool.TryParse(ret, out bool Startup);
                return Startup;
            }
            set
            {
                s_settings.Set("Startup", value.ToString());
                StartupChanged?.Invoke(null, Startup);
            }
        }
        public static string Language
        {
            get
            {
                // Note: Legacy compat, we used to write string bools.
                var ret = s_settings.Get("Language", "Auto");
                return ret;
            }
            set
            {
                s_settings.Set("Language", value);

                App._Language?.SwitchLanguage(value);

                if (value == "Auto")
                {
                    App._Language?.SwitchLanguage(System.Globalization.CultureInfo.InstalledUICulture.Name);
                }

                LanguageChanged?.Invoke(null, Language);
            }
        }
        //
        public static void RegisterHotkeys()
        {
            HotkeyManager.Current.Register(FlyoutHotkey);
            HotkeyManager.Current.Register(MixerHotkey);
            HotkeyManager.Current.Register(SettingsHotkey);
        }
    }
}