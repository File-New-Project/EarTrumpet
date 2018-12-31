using System.Collections.Generic;
using System.Collections.Specialized;

namespace EarTrumpet.DataModel
{
    public interface IAudioDeviceCollection : IEnumerable<IAudioDevice>, INotifyCollectionChanged
    {
    }
}
