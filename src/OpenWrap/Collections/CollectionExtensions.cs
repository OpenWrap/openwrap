using System.Collections.Generic;

namespace OpenWrap.Collections
{
    public static class CollectionExtensions
    {
        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> toAdd)
        {
            foreach (var value in toAdd)
                collection.Add(value);
        }

        public static void RemoveRange<T>(this ICollection<T> collection, IEnumerable<T> toRemove)
        {
            foreach (var entry in toRemove)
                collection.Remove(entry);
        }
    }
}