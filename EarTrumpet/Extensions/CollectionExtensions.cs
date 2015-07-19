using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace EarTrumpet.Extensions
{
    public static class CollectionExtensions
    {
        public static void AddRange<T>(this ICollection<T> destination, IEnumerable<T> source)
        {
            foreach (var item in source)
            {
                destination.Add(item);
            }
        }

        public static void AddSorted<T>(this ObservableCollection<T> collection, T item, IComparer<T> comparer)
        {
            var i = 0;
            while ((i < collection.Count) && (comparer.Compare(collection[i], item) < 0))
            {
                i++;
            }

            collection.Insert(i, item);
        }
    }
}
