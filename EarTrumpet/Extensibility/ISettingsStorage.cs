namespace EarTrumpet.Extensibility
{
    public interface ISettingsStorage
    {
        string Namespace { get; }
        void InitializeSettings(ISettingsBag settings);
    }
}
