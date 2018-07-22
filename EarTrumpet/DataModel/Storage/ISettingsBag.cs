namespace EarTrumpet.DataModel.Storage
{
    public interface ISettingsBag
    {
        bool HasKey(string key);
        T Get<T>(string key, T defaultValue);
        void Set<T>(string key, T value);
    }
}
