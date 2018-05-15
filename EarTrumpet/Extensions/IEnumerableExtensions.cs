using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EarTrumpet.Extensions
{
    public static class IEnumerableExtensions
    {
        public static HashSet<T> ToSet<T>(this IEnumerable<T> collection)
        {
            var ret = new HashSet<T>();
            foreach (var element in collection)
            {
                ret.Add(element);
            }
            return ret;
        }
    }
}
