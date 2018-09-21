using System;

namespace EarTrumpet.Extensibility
{
    public interface IAddonDeviceContent
    {
        object GetContentForDevice(string deviceId, Action requestClose);
    }
}
