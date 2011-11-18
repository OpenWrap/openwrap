using System.Linq;

namespace OpenWrap.Build.Tasks
{
    public static class Lookup<TKey,TValue>
    {
        static ILookup<TKey, TValue> _empty = new TValue[0].ToLookup(_ => default(TKey));

        public static ILookup<TKey, TValue> Empty { get { return _empty; } }
    }
}