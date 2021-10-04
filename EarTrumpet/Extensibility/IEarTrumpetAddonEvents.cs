namespace EarTrumpet.Extensibility
{
    public enum AddonEventKind
    {
        InitializeAddon,
        AddonsInitialized,
        AppShuttingDown,
    }

    public interface IEarTrumpetAddonEvents
    {
        void OnAddonEvent(AddonEventKind evt);
    }
}