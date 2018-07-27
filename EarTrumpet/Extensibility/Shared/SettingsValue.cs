using EarTrumpet.DataModel.Storage;

namespace EarTrumpet.Extensibility.Shared
{
    public class SettingsValue<T> : IValue<T>
    {
        public T Value
        {
            get => _settings.Get<T>(_key, default(T));
            set => _settings.Set(_key, value);
        }

        public string Id => _settings.Namespace + _key;

        public string DisplayName { get; }

        private ISettingsBag _settings;
        private string _key;

        public SettingsValue(string displayName, string key, ISettingsBag settings)
        {
            DisplayName = displayName;
            _key = key;
            _settings = settings;
        }
    }
}
