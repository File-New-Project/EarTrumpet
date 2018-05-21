using System.Collections.Specialized;

namespace EarTrumpet.DataModel
{
    interface IVirtualDefaultAudioDevice : IAudioDevice
    {
        bool IsDevicePresent { get; }
        event NotifyCollectionChangedEventHandler CollectionChanged;
    }
}