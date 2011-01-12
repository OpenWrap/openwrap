using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OpenWrap.PackageModel.Parsers
{
    public class ScopedPackageDependencyCollection : ICollection<PackageDependency>
    {
        readonly PackageDependencyCollection _parent;
        readonly PackageDependencyCollection _current;

        public ScopedPackageDependencyCollection(PackageDependencyCollection parent, PackageDependencyCollection current)
        {
            _parent = parent;
            _current = current;
        }

        public IEnumerator<PackageDependency> GetEnumerator()
        {
            var currentKeys = _current.Select(x => x.Name).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            foreach (var val in _current.Concat(_parent.Where(p => currentKeys.Contains(p.Name, StringComparer.OrdinalIgnoreCase) == false)))
                yield return val;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(PackageDependency item)
        {
            _current.Add(item);
        }

        public void Clear()
        {
            _current.Clear();
        }

        public bool Contains(PackageDependency item)
        {
            return _current.Contains(item) || _parent.Contains(item);
        }

        public void CopyTo(PackageDependency[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public bool Remove(PackageDependency item)
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