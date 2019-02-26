using System;
using System.Collections.ObjectModel;

namespace EarTrumpet.DataModel
{
    public class FilteredCollectionChain<T>
    {
        ObservableCollection<T> _items;

        public ObservableCollection<T> Items { get; }

        public FilteredCollectionChain(ObservableCollection<T> items)
        {
            Items = new ObservableCollection<T>();

            _items = items;

            Listen();
            Populate();
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
            for (var i = Items.Count - 1; i >= 0; i--)
            {
                Items.RemoveAt(i);
            }

            _items = filter(_items);
            Listen();
            Populate();
        }

        private void Populate()
        {
            foreach(var s in _items)
            {
                Items.Add(s);
            }
        }
    }
}
