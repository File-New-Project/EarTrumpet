using System;

namespace EarTrumpet.Extensibility
{
    public interface IAddonAppContent
    {
        object GetContentForApp(string deviceId, string appId, Action requestClose);
    }
}
