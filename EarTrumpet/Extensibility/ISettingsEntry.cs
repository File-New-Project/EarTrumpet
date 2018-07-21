using EarTrumpet.UI.Services;

namespace EarTrumpet.Extensibility
{
    public interface ISettingsWindowHost
    {
        HotkeyData GetHotkeyFromUser();
    }

    public interface ISettingsEntry
    {
        void Advise(ISettingsWindowHost host);
        string DisplayName { get; }
        object Content { get; }
    }
}
