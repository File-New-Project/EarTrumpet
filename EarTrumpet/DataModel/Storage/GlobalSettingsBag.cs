using EarTrumpet.Extensions;
using Microsoft.Win32;
using System;
using System.Diagnostics;

namespace EarTrumpet.DataModel.Storage
{
    class GlobalSettingsBag : ISettingsBag
    {
        public event EventHandler<string> SettingChanged;

        public bool HasKey(string key)
        {
            var ret = false;
            if (App.Current.HasIdentity())
            {
                try
                {
                    ret = Windows.Storage.ApplicationData.Current.LocalSettings.Values.ContainsKey(key);
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
                    ret = regKey.GetValue(key) != null;
                }
            }
            return ret;
        }

        public T Get<T>(string key, T defaultValue)
        {
            if ((defaultValue is bool && App.Current.HasIdentity()) ||
                defaultValue is string)
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
            if ((value is bool && App.Current.HasIdentity())
                || value is string)
            {
                WriteSetting<T>(key, value);
                return;
            }

            WriteSetting(key, Serializer.ToString(key, value));

            SettingChanged?.Invoke(this, key);
        }

        static T ReadSetting<T>(string key)
        {
            T ret = default(T);

            if (App.Current.HasIdentity())
            {
                try
                {
                    ret = (T)Windows.Storage.ApplicationData.Current.LocalSettings.Values[key];
                }
                catch (Exception ex)
                {
                    Trace.TraceError($"{ex}");
                    AppTrace.LogWarning(ex);
                }
            }
            else
            {
                using (var regKey = Registry.CurrentUser.CreateSubKey(@"Software\EarTrumpet", true))
                {
                    ret = (T)regKey.GetValue(key);
                }
            }
            return ret;
        }

        static void WriteSetting<T>(string key, T value)
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
