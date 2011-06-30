using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OpenWrap.Collections
{
    public class IndexedDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        readonly IComparer<TKey> _comparer;
        readonly Func<TValue, TKey> _keyReader;
        readonly Action<TKey, TValue> _keyWriter;
        readonly List<TValue> _list;

        public IndexedDictionary(Func<TValue, TKey> keyReader, Action<TKey, TValue> keyWriter)
            : this(keyReader, keyWriter, Comparer<TKey>.Default)
        {
        }

        public IndexedDictionary(Func<TValue, TKey> keyReader, Action<TKey, TValue> keyWriter, IComparer<TKey> comparer)
        {
            _keyReader = val => ReferenceEquals(val, null) ? default(TKey) : keyReader(val);
            _keyWriter = (key, val) => { if (ReferenceEquals(val, null) == false) keyWriter(key, val); };
            _comparer = comparer;
            _list = new List<TValue>();
        }

        public int Count
        {
            get { return _list.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public ICollection<TKey> Keys
        {
            get { return _list.Select(item => _keyReader(item)).ToList(); }
        }

        public ICollection<TValue> Values
        {
            get { return _list; }
        }

        public TValue this[TKey key]
        {
            get
            {
                if (!ContainsKey(key))
                    throw new ArgumentException("key not found");
                return _list.SingleOrDefault(x => MatchesKey(x, key));
            }
            set
            {
                Remove(key);
                Add(value);
            }
        }

        public void Add(TValue value)
        {
            if (ContainsKey(_keyReader(value)))
                throw new ArgumentException("Key already present");
            _list.Add(value);
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            _keyWriter(item.Key, item.Value);
            Add(item.Value);
        }

        public void Clear()
        {
            _list.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            TValue val;
            return TryGetValue(item.Key, out val) && val.Equals(item.Value);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException("array");
            if (arrayIndex < 0) throw new ArgumentOutOfRangeException("array");
            if (_list.Count > array.Length - arrayIndex) throw new ArgumentException("arrayIndex");

            _list.Select(_ => new KeyValuePair<TKey, TValue>(_keyReader(_), _)).ToList().CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (!Contains(item))
                throw new ArgumentException();
            return Remove(item.Key);
        }

        public void Add(TKey key, TValue value)
        {
            _keyWriter(key, value);
            Add(value);
        }

        public bool ContainsKey(TKey key)
        {
            return _list.Any(x => MatchesKey(x, key));
        }

        public bool Remove(TKey key)
        {
            return _list.RemoveAll(_ => MatchesKey(_, key)) > 0;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (_list.Any(x => MatchesKey(x, key)))
            {
                value = _list.Single(x => MatchesKey(x, key));
                return true;
            }
            value = default(TValue);
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _list.Select(_ => new KeyValuePair<TKey, TValue>(_keyReader(_), _)).GetEnumerator();
        }

        bool MatchesKey(TValue value, TKey key)
        {
            return _comparer.Compare(_keyReader(value), key) == 0;
        }
    }
}