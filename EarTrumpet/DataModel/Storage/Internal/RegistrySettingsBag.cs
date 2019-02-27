using Microsoft.Win32;
using System;

namespace EarTrumpet.DataModel.Storage.Internal
{
    class RegistrySettingsBag : ISettingsBag
    {
        private static readonly string s_earTrumpetKey = @"Software\EarTrumpet";

        public string Namespace => "";

        public event EventHandler<string> SettingChanged;

        public bool HasKey(string key)
        {
            using (var regKey = Registry.CurrentUser.CreateSubKey(s_earTrumpetKey, true))
            {
                return regKey.GetValue(key) != null;
            }
        }

        public T Get<T>(string key, T defaultValue)
        {
            if (defaultValue is string)
            {
                return ReadSetting<T>(key);
            }

            var data = ReadSetting<string>(key);
            if (string.IsNullOrWhiteSpace(data))
            {
                return defaultValue;
            }

            return Serializer.FromString<T>(data);
        }

        public void Set<T>(string key, T value)
        {
            if (value is string)
            {
                WriteSetting<T>(key, value);
            }
            else
            {
                WriteSetting(key, Serializer.ToString(key, value));
            }

            SettingChanged?.Invoke(this, key);
        }

        static T ReadSetting<T>(string key)
        {
            using (var regKey = Registry.CurrentUser.CreateSubKey(s_earTrumpetKey, true))
            {
                return (T)regKey.GetValue(key);
            }
        }

        static void WriteSetting<T>(string key, T value)
        {
            using (var regKey = Registry.CurrentUser.CreateSubKey(s_earTrumpetKey, true))
            {
                regKey.SetValue(key, value);
            }
        }
    }
}
