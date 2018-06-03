using EarTrumpet.Extensions;
using EarTrumpet.Misc;
using Microsoft.Win32;
using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace EarTrumpet.Services
{
    public class SettingsService
    {
        public static event EventHandler<bool> UseLegacyIconChanged;

        public class HotkeyData
        {
            public KeyboardHook.ModifierKeys Modifiers;
            public System.Windows.Forms.Keys Key;

            public override string ToString()
            {
                string ret = "";

                if ((Modifiers & KeyboardHook.ModifierKeys.Control) == KeyboardHook.ModifierKeys.Control)
                {
                    ret += "Ctrl+";
                }
                if ((Modifiers & KeyboardHook.ModifierKeys.Shift) == KeyboardHook.ModifierKeys.Shift)
                {
                    ret += "Shift+";
                }
                if ((Modifiers & KeyboardHook.ModifierKeys.Alt) == KeyboardHook.ModifierKeys.Alt)
                {
                    ret += "Alt+";
                }

                if (Key != System.Windows.Forms.Keys.None)
                {
                    ret += Key.ToString();
                }

                return ret;
            }
        }

        public static HotkeyData Hotkey
        {
            get
            {
                var ret = ReadSetting("Hotkey", new HotkeyData { Modifiers = KeyboardHook.ModifierKeys.Shift | KeyboardHook.ModifierKeys.Control, Key = System.Windows.Forms.Keys.Q });
                return ret;
            }
            set
            {
                WriteSetting("Hotkey", value);
            }
        }

        public static bool UseLegacyIcon
        {
            get
            {
                var ret = ReadSetting("UseLegacyIcon");
                if (ret == null)
                {
                    return false;
                }

                bool.TryParse(ret, out bool isUseLegacyIcon);
                return isUseLegacyIcon;
            }
            set
            {
                WriteSetting("UseLegacyIcon", value.ToString());

                UseLegacyIconChanged?.Invoke(null, UseLegacyIcon);
            }
        }


        static string ReadSetting(string key)
        {
            string ret;

            if (App.Current.HasIdentity())
            {
                ret = (string)Windows.Storage.ApplicationData.Current.LocalSettings.Values[key];
            }
            else
            {
                using (var regKey = Registry.CurrentUser.CreateSubKey(@"Software\EarTrumpet", true))
                {
                    ret = (string)regKey.GetValue(key);
                }
            }

            return ret;
        }

        static T ReadSetting<T>(string key, T defaultValue) where T : class
        {
            var data = ReadSetting(key);
            if (string.IsNullOrWhiteSpace(data))
            {
                return defaultValue;
            }

            using (TextReader reader = new StringReader(data))
            {
                XmlSerializer xs = new XmlSerializer(typeof(T));
                return (T)xs.Deserialize(reader);
            }
        }

        static void WriteSetting<T>(string key, T value)
        {
            var xmlserializer = new XmlSerializer(typeof(T));
            var stringWriter = new StringWriter();
            using (var writer = XmlWriter.Create(stringWriter))
            {
                xmlserializer.Serialize(writer, value);
                WriteSetting(key, stringWriter.ToString());
            }
        }

        static void WriteSetting(string key, string value)
        {
            if (App.Current.HasIdentity())
            {
                Windows.Storage.ApplicationData.Current.LocalSettings.Values[key] = value;
            }
            else
            {
                using (var regKey = Registry.CurrentUser.CreateSubKey(@"Software\EarTrumpet", true))
                {
                    regKey.SetValue(key, value);
                }
            }
        }
    }
}
