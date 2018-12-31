using EarTrumpet.DataModel.Storage;

namespace EarTrumpet.Extensibility.Shared
{
    public class SettingsValue<T> : IValue<T>
    {
        public T Value
        {
            get => _simple.Value;
            set => _simple.Value = value;
        }

        public string Id => _simple.Id;

        public string DisplayName => _simple.DisplayName;

        private SimpleValue<T> _simple;

        public SettingsValue(string displayName, string key, ISettingsBag settings)
        {
            _simple = new SimpleValue<T>(
                settings.Namespace + key,
                displayName,
                () => settings.Get(key, default(T)),
                (v) => settings.Set(key, v));
        }
    }
}
