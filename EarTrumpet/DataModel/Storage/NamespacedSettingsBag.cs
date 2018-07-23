using System;

namespace EarTrumpet.DataModel.Storage
{
    class NamespacedSettingsBag : ISettingsBag
    {
        public event EventHandler<string> SettingChanged;

        private readonly string _nameSpace;
        private readonly ISettingsBag _globalBag;

        public NamespacedSettingsBag(string nameSpace, ISettingsBag bag)
        {
            _nameSpace = nameSpace;
            _globalBag = bag;
        }


        public T Get<T>(string key, T defaultValue)
        {
            return _globalBag.Get($"{_nameSpace}.{key}", defaultValue);
        }

        public bool HasKey(string key)
        {
            return _globalBag.HasKey($"{_nameSpace}.{key}");
        }

        public void Set<T>(string key, T value)
        {
            _globalBag.Set($"{_nameSpace}.{key}", value);
            SettingChanged?.Invoke(this, key);
        }
    }
}