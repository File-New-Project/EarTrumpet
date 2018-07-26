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
        void OnApplicationLifecycleEvent(ApplicationLifecycleEvent evt);
    }
}