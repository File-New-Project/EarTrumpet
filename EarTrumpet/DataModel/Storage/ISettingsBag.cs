using System;

namespace EarTrumpet.DataModel.Storage
{
    public interface ISettingsBag
    {
        string Namespace { get; }
        bool HasKey(string key);
        T Get<T>(string key, T defaultValue);
        void Set<T>(string key, T value);

        event EventHandler<string> SettingChanged;
    }
}
