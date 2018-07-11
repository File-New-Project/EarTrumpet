using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace EarTrumpet.DataModel.Internal
{
    class AudioDeviceCollection : IAudioDeviceCollection
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private List<IAudioDevice> _devices = new List<IAudioDevice>();
        private object _lock = new object();

        public IEnumerator<IAudioDevice> GetEnumerator()
        {
            lock (_lock)
            {
                return _devices.ToList().GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((AudioDeviceCollection)this).GetEnumerator();
        }

        public void Add(AudioDevice device)
        {
            lock (_lock)
            {
                _devices.Add(device);
            }
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, device));
        }

        public void Remove(IAudioDevice device)
        {
            lock (_lock)
            {
                _devices.Remove(device);
            }
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, device));
        }
    }
}
