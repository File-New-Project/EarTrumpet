using System;
using System.Collections.ObjectModel;

namespace EarTrumpet.DataModel.Internal
{
    class FilteredCollectionChain<T>
    {
        ObservableCollection<T> _sessions;

        public ObservableCollection<T> Sessions { get; }

        public FilteredCollectionChain(ObservableCollection<T> sessions)
        {
            Sessions = new ObservableCollection<T>();

            _sessions = sessions;

            Listen();
            Populate();
        }

        void Listen()
        {
            _sessions.CollectionChanged += Sessions_CollectionChanged;
        }

        void Free()
        {
            _sessions.CollectionChanged -= Sessions_CollectionChanged;
        }

        private void Sessions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    Sessions.Add((T)e.NewItems[0]);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    Sessions.Remove((T)e.OldItems[0]);
                    break;
                default: throw new NotImplementedException();
            }
        }

        public void AddFilter(Func<ObservableCollection<T>, ObservableCollection<T>> filter)
        {
            Free();
            for (var i = _sessions.Count - 1; i >= 0; i--)
            {
                _sessions.RemoveAt(i);
            }

            _sessions = filter(_sessions);
            Listen();
            Populate();
        }

        private void Populate()
        {
            foreach(var s in _sessions)
            {
                Sessions.Add(s);
            }
        }
    }
}
