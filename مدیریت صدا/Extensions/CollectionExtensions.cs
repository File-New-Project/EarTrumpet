using System.Collections.Generic;
using System.Collections.ObjectModel;

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
    }
}
