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
        readonly List<string> _removed;

        public ScopedPackageDependencyCollection(PackageDependencyCollection parent, PackageDependencyCollection current)
        {
            _parent = parent;
            _current = current;
            _removed = new List<string>();
        }

        public IEnumerator<PackageDependency> GetEnumerator()
        {
            var overriddenPackages = _current.Select(x => x.Name).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            foreach (var val in _current.Concat(_parent.Where(p => !overriddenPackages.ContainsNoCase(p.Name) && !_removed.ContainsNoCase(p.Name))))
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
            return _current.Contains(item) || (_parent.Contains(item) && !_removed.ContainsNoCase(item.Name));
        }

        public void CopyTo(PackageDependency[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public bool Remove(PackageDependency item)
        {
            if (_current.Contains(item))
                _current.Remove(item);
            if (_removed.ContainsNoCase(item.Name)) return true;
            if (_parent.Any(_=>_.Name.EqualsNoCase(item.Name)))
            {
                _removed.Add(item.Name);
                return true;
            }
            return false;
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