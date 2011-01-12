using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OpenWrap.PackageModel.Parsers
{
    public class ScopedPackageNameOverrideCollection : ICollection<PackageNameOverride>
    {
        readonly PackageNameOverrideCollection _parent;
        readonly PackageNameOverrideCollection _current;

        public ScopedPackageNameOverrideCollection(PackageNameOverrideCollection parent, PackageNameOverrideCollection current)
        {
            _parent = parent;
            _current = current;
        }

        public IEnumerator<PackageNameOverride> GetEnumerator()
        {
            var currentKeys = _current.Select(x => x.OldPackage).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            foreach (var val in _current.Concat(_parent.Where(p => currentKeys.Contains(p.OldPackage, StringComparer.OrdinalIgnoreCase) == false)))
                yield return val;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(PackageNameOverride item)
        {
            _current.Add(item);
        }

        public void Clear()
        {
            _current.Clear();
        }

        public bool Contains(PackageNameOverride item)
        {
            return _current.Contains(item) || _parent.Contains(item);
        }

        public void CopyTo(PackageNameOverride[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public bool Remove(PackageNameOverride item)
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