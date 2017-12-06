using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace EarTrumpet.Services
{
    public class SettingsService
    {
        public class DefaultApp
        {
            public string Id;
            public string IconPath;
            public string DisplayName;
            public float Volume;
            public bool IsMuted;
            public uint BackgroundColor;
            public bool IsDesktopApp;
        }

        public static DefaultApp[] DefaultApps
        {
            get
            {
                var ret = ReadSetting<DefaultApp[]>("DefaultApps", new DefaultApp[] { });
                return ret;
            }
            set
            {
                WriteSetting("DefaultApps", value);
            }
        }

        static string ReadSetting(string key)
        {
            string ret;

            if (App.HasIdentity())
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
            if (App.HasIdentity())
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
