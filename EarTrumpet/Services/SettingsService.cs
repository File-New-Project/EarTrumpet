using Microsoft.Win32;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace EarTrumpet.Services
{
    public class SettingsService
    {
        public class HotkeyData
        {
            public KBModifierKeys Modifiers;
            public System.Windows.Forms.Keys Key;

            public override string ToString()
            {
                string ret = "";

                if ((Modifiers & KBModifierKeys.Control) == KBModifierKeys.Control)
                {
                    ret += "Control+";
                }
                if ((Modifiers & KBModifierKeys.Shift) == KBModifierKeys.Shift)
                {
                    ret += "Shift+";
                }
                if ((Modifiers & KBModifierKeys.Alt) == KBModifierKeys.Alt)
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
                var ret = ReadSetting("Hotkey", new HotkeyData { Modifiers = KBModifierKeys.Shift | KBModifierKeys.Control, Key = System.Windows.Forms.Keys.Q });
                return ret;
            }
            set
            {
                WriteSetting("Hotkey", value);
            }
        }
        
        static string ReadSetting(string key)
        {
            string ret;

            if (App.HasIdentity)
            {
                ret = (string)Windows.Storage.ApplicationData.Current.LocalSettings.Values[key];
            }
            else
            {
                ret = (string)Registry.CurrentUser.CreateSubKey(@"Software\EarTrumpet", true).GetValue(key);
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
                return xs.Deserialize(reader) as T;
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
            if (App.HasIdentity)
            {
                Windows.Storage.ApplicationData.Current.LocalSettings.Values[key] = value;
            }
            else
            {
                Registry.CurrentUser.CreateSubKey(@"Software\EarTrumpet", true).SetValue(key, value);
            }
        }
    }
}
