using EarTrumpet.DataModel.Storage;
using EarTrumpet.Interop.Helpers;
using System;

namespace EarTrumpet.UI.Services
{
    public class SettingsService
    {
        public static event EventHandler<bool> UseLegacyIconChanged;

        private static readonly string s_trayIconIdKey = "TrayIconId";
        private static ISettingsBag s_settings = StorageFactory.GetSettings();

        public static Guid TrayIconIdentity
        {
            get
            {
                var id = s_settings.Get(s_trayIconIdKey, Guid.Empty);
                if (id == Guid.Empty)
                {
                    id = Guid.NewGuid();
                    s_settings.Set(s_trayIconIdKey, id);
                }
                return id;
            }
        }

        public static void ResetTrayIconIdentity() => s_settings.Set(s_trayIconIdKey, Guid.Empty);

        public static HotkeyData FlyoutHotkey
        {
            get => s_settings.Get("Hotkey", new HotkeyData { });
            set
            {
                HotkeyManager.Current.Unregister(FlyoutHotkey);
                s_settings.Set("Hotkey", value);
                HotkeyManager.Current.Register(FlyoutHotkey);
            }
        }

        public static HotkeyData MixerHotkey
        {
            get => s_settings.Get("MixerHotkey", new HotkeyData { });
            set
            {
                HotkeyManager.Current.Unregister(MixerHotkey);
                s_settings.Set("MixerHotkey", value);
                HotkeyManager.Current.Register(MixerHotkey);
            }
        }

        public static HotkeyData SettingsHotkey
        {
            get => s_settings.Get("SettingsHotkey", new HotkeyData { });
            set
            {
                HotkeyManager.Current.Unregister(SettingsHotkey);
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

        public static void RegisterHotkeys()
        {
            HotkeyManager.Current.Register(FlyoutHotkey);
            HotkeyManager.Current.Register(MixerHotkey);
            HotkeyManager.Current.Register(SettingsHotkey);
        }
    }
}