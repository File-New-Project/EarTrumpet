using System.Collections.Generic;
using System.Collections.Specialized;

namespace EarTrumpet.DataModel
{
    interface IAudioDeviceCollection : IEnumerable<IAudioDevice>, INotifyCollectionChanged
    {
    }
}
