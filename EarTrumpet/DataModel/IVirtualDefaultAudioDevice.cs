using System.Collections.Specialized;

namespace EarTrumpet.DataModel
{
    public interface IVirtualDefaultAudioDevice : IAudioDevice
    {
        bool IsDevicePresent { get; }
        event NotifyCollectionChangedEventHandler CollectionChanged;
    }
}