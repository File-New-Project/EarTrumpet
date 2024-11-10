using EarTrumpet.Diagnosis;
using System;
using Windows.Management.Core;
using Windows.Storage;

namespace EarTrumpet.DataModel.Storage.Internal;

internal class WindowsStorageSettingsBag : ISettingsBag
{
    private static readonly ApplicationData _appDataManager = ApplicationDataManager.CreateForPackageFamily(App.PackageName);

    public string Namespace => "";
    public event EventHandler<string> SettingChanged;

    public bool HasKey(string key)
    {
        var ret = false;
        try
        {
            ret = _appDataManager.LocalSettings.Values.ContainsKey(key);
        }
        catch (Exception ex)
        {
            ErrorReporter.LogWarning(ex);
        }
        return ret;
    }

    public T Get<T>(string key, T defaultValue)
    {
        if (!HasKey(key))
        {
            return defaultValue;
        }

        if (defaultValue is bool || defaultValue is string)
        {
            return ReadSetting<T>(key, defaultValue);
        }

        var data = ReadSetting<string>(key, null);
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

    private static T ReadSetting<T>(string key, T defaultValue)
    {
        var ret = defaultValue;
        try
        {
            ret = (T)_appDataManager.LocalSettings.Values[key];
        }
        catch (Exception ex)
        {
            ErrorReporter.LogWarning(ex);
        }
        return ret;
    }

    private static void WriteSetting<T>(string key, T value)
    {
        try
        {
            _appDataManager.LocalSettings.Values[key] = value;
        }
        catch (Exception ex)
        {
            ErrorReporter.LogWarning(ex);
        }
    }
}
