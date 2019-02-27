using System;

namespace EarTrumpet.DataModel.Storage.Internal
{
    class NamespacedSettingsBag : ISettingsBag
    {
        public string Namespace { get; }

        public event EventHandler<string> SettingChanged;

        private readonly ISettingsBag _globalBag;

        public NamespacedSettingsBag(string nameSpace, ISettingsBag bag)
        {
            Namespace = nameSpace + ".";
            _globalBag = bag;
        }


        public T Get<T>(string key, T defaultValue)
        {
            return _globalBag.Get($"{Namespace}{key}", defaultValue);
        }

        public bool HasKey(string key)
        {
            return _globalBag.HasKey($"{Namespace}{key}");
        }

        public void Set<T>(string key, T value)
        {
            _globalBag.Set($"{Namespace}{key}", value);
            SettingChanged?.Invoke(this, key);
        }
    }
}