namespace EarTrumpet.Extensibility
{
    // Not to be implemented in add-on.
    public interface ISettingsBag
    {
        T Get<T>(string key, T defaultValue);
        void Set<T>(string key, T value);
    }
}
