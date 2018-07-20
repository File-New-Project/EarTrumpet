namespace EarTrumpet.Extensibility
{
    public interface ISettingsEntry
    {
        string DisplayName { get; }
        object Content { get; }
    }
}
