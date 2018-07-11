using System.Collections.Generic;
using System.Collections.Specialized;
using EarTrumpet.DataModel.Internal;

namespace EarTrumpet.DataModel
{
    interface IAudioDeviceCollection : IEnumerable<IAudioDevice>, INotifyCollectionChanged
    {
        void Add(AudioDevice newDevice);
        void Remove(IAudioDevice dev);
    }
}
