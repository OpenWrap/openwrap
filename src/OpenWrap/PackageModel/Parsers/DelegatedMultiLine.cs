using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OpenWrap.PackageModel.Parsers
{
    public class DelegatedMultiLine<T> : ICollection<T>
    {
        readonly MultiLine<T> _parent;
        readonly MultiLine<T> _current;

        public DelegatedMultiLine(MultiLine<T> parent, MultiLine<T> current)
        {
            _parent = parent;
            _current = current;
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var value in _current.Concat(_parent))
                yield return value;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item)
        {
            _current.Add(item);

        }

        public void Clear()
        {
            _current.Clear();
        }

        public bool Contains(T item)
        {
            return _current.Contains(item) || _parent.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public bool Remove(T item)
        {
            return _current.Remove(item);
        }

        public int Count
        {
            get { return _current.Count + _parent.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }
    }
}