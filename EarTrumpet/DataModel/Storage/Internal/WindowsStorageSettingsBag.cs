using EarTrumpet.Diagnosis;
using System;

namespace EarTrumpet.DataModel.Storage.Internal
{
    class WindowsStorageSettingsBag : ISettingsBag
    {
        public string Namespace => "";

        public event EventHandler<string> SettingChanged;

        public bool HasKey(string key)
        {
            var ret = false;
            try
            {
                ret = Windows.Storage.ApplicationData.Current.LocalSettings.Values.ContainsKey(key);
            }
            catch (Exception ex)
            {
                ErrorReporter.LogWarning(ex);
            }
            return ret;
        }

        public T Get<T>(string key, T defaultValue)
        {
            if (defaultValue is bool || defaultValue is string)
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
            if (value is bool || value is string)
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
            T ret = default(T);
            try
            {
                ret = (T)Windows.Storage.ApplicationData.Current.LocalSettings.Values[key];
            }
            catch (Exception ex)
            {
                // Windows Bug: Windows Storage APIs are still unreliable
                ErrorReporter.LogWarning(ex);
            }
            return ret;
        }

        static void WriteSetting<T>(string key, T value)
        {
            try
            {
                Windows.Storage.ApplicationData.Current.LocalSettings.Values[key] = value;
            }
            catch (Exception ex)
            {
                // Windows Bug: Windows Storage APIs are still unreliable
                ErrorReporter.LogWarning(ex);
            }
        }
    }
}
