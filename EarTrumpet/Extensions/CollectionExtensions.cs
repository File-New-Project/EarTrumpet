using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace EarTrumpet.Extensions
{
    public static class CollectionExtensions
    {
        public static void AddSorted<T>(this ObservableCollection<T> collection, T item, IComparer<T> comparer)
        {
            var i = 0;
            while ((i < collection.Count) && (comparer.Compare(collection[i], item) < 0))
            {
                i++;
            }

            collection.Insert(i, item);
        }

        public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                collection.Add(item);
            }
        }

        public static void InsertRange<T>(this ObservableCollection<T> collection, int startIndex, IEnumerable<T> items)
        {
            foreach (var item in items.Reverse())
            {
                collection.Insert(startIndex, item);
            }
        }
    }
}
