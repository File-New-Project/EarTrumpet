using System;
using System.Collections.Generic;
using System.Diagnostics;

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

        public static void ForEachNoThrow<T>(this IEnumerable<T> list, Action<T> action)
        {
            foreach (var item in list)
            {
                try
                {
                    action(item);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"ForEachNoThrow: {ex}");
                }
            }
        }
    }
}
