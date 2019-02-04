using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EarTrumpet.Extensions
{
    public static class ListExtensions
    {
        public static void Remove<T>(this List<T> list, Func<T,bool> shouldRemove)
        {
            var toRemove = new List<T>();
            foreach(var entry in list)
            {
                if (shouldRemove(entry))
                {
                    toRemove.Add(entry);
                }
            }

            foreach(var entry in toRemove)
            {
                list.Remove(entry);
            }
        }
    }
}
