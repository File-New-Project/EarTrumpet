using System.Collections.Generic;

namespace EarTrumpet.DataModel
{
    public interface IAudioDeviceWindowsAudio : IAudioDevice
    {
        IEnumerable<IAudioDeviceChannel> Channels { get; }
        string EnumeratorName { get; }
        string InterfaceName { get; }
        string DeviceDescription { get; }
    }
}
