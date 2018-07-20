namespace EarTrumpet.Extensibility
{
    public enum ApplicationLifecycleEvent
    {
        Startup,
        Shutdown,
    }

    public interface IApplicationLifecycle
    {
        void OnApplicationLifecycleEvent(ApplicationLifecycleEvent evt);
    }
}
