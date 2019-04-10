namespace EarTrumpet.Extensibility
{
    public enum ApplicationLifecycleEvent
    {
        Startup,
        Startup2,
        Shutdown,
    }

    public interface IAddonLifecycle
    {
        AddonInfo Info { get; }

        void OnApplicationLifecycleEvent(ApplicationLifecycleEvent evt);
    }
}