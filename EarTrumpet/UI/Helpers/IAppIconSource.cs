namespace EarTrumpet.UI.Helpers
{
    public interface IAppIconSource
    {
        bool IsDesktopApp { get; }
        string IconPath { get; }
    }
}
