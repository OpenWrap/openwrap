using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenWrap
{
    public static class CollectionExtensions
    {
        public static void RemoveRange<T>(this ICollection<T> collection, IEnumerable<T> toRemove)
        {
            foreach(var entry in toRemove)
                collection.Remove(entry);
        }
        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> toAdd)
        {
            foreach(var value in toAdd)
                collection.Add(value);
        }
    }
}
