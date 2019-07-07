using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;

namespace EarTrumpet.DataModel
{
    public class FilteredCollectionChain<T>
    {
        ObservableCollection<T> _items;

        public ObservableCollection<T> Items { get; }

        public FilteredCollectionChain(ObservableCollection<T> items, Dispatcher foregroundDispatcher)
        {
            Items = new ObservableCollection<T>();

            _items = items;

            foregroundDispatcher.BeginInvoke((Action)(() => {
                Listen();
                Populate();
            }));
        }

        void Listen()
        {
            _items.CollectionChanged += Sessions_CollectionChanged;
        }

        void Free()
        {
            _items.CollectionChanged -= Sessions_CollectionChanged;
        }

        private void Sessions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    Items.Add((T)e.NewItems[0]);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    Items.Remove((T)e.OldItems[0]);
                    break;
                default: throw new NotImplementedException();
            }
        }

        public void AddFilter(Func<ObservableCollection<T>, ObservableCollection<T>> filter)
        {
            Free();
            _items = filter(_items);
            Listen();
            Populate();
        }

        private void Populate()
        {
            foreach(var s in _items)
            {
                if (!Items.Contains(s))
                {
                    Items.Add(s);
                }
            }

            var filtered = Items.Where(i => !_items.Contains(i)).ToArray();
            foreach(var item in filtered)
            {
                Items.Remove(item);
            }
        }
    }
}
