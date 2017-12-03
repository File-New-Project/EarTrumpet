using System.Collections.ObjectModel;
using System.Linq;

namespace EarTrumpet.DataModel.Extensions
{
    public static class CollectionExtensionsForAudioDeviceSession
    {
        public static bool ContainsById(this Collection<IAudioDeviceSession> collection, IAudioDeviceSession session)
        {
            return collection.Any(c => c.GroupingParam == session.GroupingParam);
        }

        public static void RemoveById(this Collection<IAudioDeviceSession> collection, IAudioDeviceSession session)
        {
            var ret = collection.First(c => c.GroupingParam == session.GroupingParam);
            collection.Remove(ret);
        }
    }
}
