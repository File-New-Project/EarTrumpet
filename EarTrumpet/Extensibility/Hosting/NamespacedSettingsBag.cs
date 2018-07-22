using EarTrumpet.Extensibility;

namespace EarTrumpet.Extensibility.Hosting
{
    class NamespacedSettingsBag : ISettingsBag
    {
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

        public void Set<T>(string key, T value)
        {
            _globalBag.Set($"{_nameSpace}.{key}", value);
        }
    }
}