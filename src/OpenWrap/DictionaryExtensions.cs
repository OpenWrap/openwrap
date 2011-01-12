using System.Collections.Generic;

namespace OpenWrap
{
    public static class DictionaryExtensions
    {
        public static TValue Get<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            return dictionary.ContainsKey(key) ? dictionary[key] : default(TValue);
        }

        public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
                where TValue : new()
        {
            TValue outValue;
            if (!dictionary.TryGetValue(key, out outValue))
                dictionary.Add(key, outValue = new TValue());
            return outValue;
        }
    }
}