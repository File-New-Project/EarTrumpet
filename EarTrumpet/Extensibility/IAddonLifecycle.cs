namespace EarTrumpet.Extensibility
{
    public enum ApplicationLifecycleEvent
    {
        Startup,
        Shutdown,
    }

    public interface IAddonLifecycle
    {
        void OnApplicationLifecycleEvent(ApplicationLifecycleEvent evt);
    }
}