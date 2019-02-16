using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace EarTrumpet.DataModel.Internal
{
    public class AudioDeviceCollection : IAudioDeviceCollection
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private readonly ConcurrentDictionary<string, IAudioDevice> _devices = new ConcurrentDictionary<string, IAudioDevice>();

        public void Add(IAudioDevice device)
        {
            if (_devices.TryAdd(device.Id, device))
            {
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, device));
            }
        }

        public void Remove(IAudioDevice device)
        {
            if (_devices.TryRemove(device.Id, out var foundDevice))
            {
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, foundDevice));
            }
        }

        public bool TryFind(string deviceId, out IAudioDevice found)
        {
            if (deviceId == null)
            {
                found = null;
                return false;
            }

            return _devices.TryGetValue(deviceId, out found);
        }

        public IEnumerator<IAudioDevice> GetEnumerator() => _devices.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}
