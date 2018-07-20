using EarTrumpet.Extensibility;
using EarTrumpet.Extensions;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace EarTrumpet.Hosting
{
    class GlobalSettingsBag : ISettingsBag
    {
        public T Get<T>(string key, T defaultValue)
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

        public void Set<T>(string key, T value)
        {
            var xmlserializer = new XmlSerializer(typeof(T));
            var stringWriter = new StringWriter();
            using (var writer = XmlWriter.Create(stringWriter))
            {
                xmlserializer.Serialize(writer, value);
                WriteSetting(key, stringWriter.ToString());
            }
        }

        static string ReadSetting(string key)
        {
            string ret = null;

            if (App.Current.HasIdentity())
            {
                try
                {
                    ret = (string)Windows.Storage.ApplicationData.Current.LocalSettings.Values[key];
                }
                catch (Exception ex)
                {
                    Trace.TraceError($"{ex}");
                    AppTrace.LogWarning(ex);
                    return ret;
                }
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

        static void WriteSetting(string key, string value)
        {
            if (App.Current.HasIdentity())
            {
                try
                {
                    Windows.Storage.ApplicationData.Current.LocalSettings.Values[key] = value;
                }
                catch (Exception ex)
                {
                    // Windows Bug: Windows Storage APIs are still unreliable
                    Trace.TraceError($"{ex}");
                    AppTrace.LogWarning(ex);
                }
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
