using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.Collections;

namespace OpenWrap.PackageModel
{
    public class PackageDescriptorEntryCollection : IEnumerable<IPackageDescriptorEntry>
    {
        readonly Dictionary<string, List<IPackageDescriptorEntry>> _byName = new Dictionary<string, List<IPackageDescriptorEntry>>(StringComparer.OrdinalIgnoreCase);
        readonly List<IPackageDescriptorEntry> _headers = new List<IPackageDescriptorEntry>();

        public PackageDescriptorEntryCollection()
        {
            
        }

        public PackageDescriptorEntryCollection(IEnumerable<IPackageDescriptorEntry> entry)
        {
            
        }
        public IPackageDescriptorEntry Append(IPackageDescriptorEntry entry)
        {
            if (!_headers.Contains(entry))
                _headers.Add(entry);
            GetOrAddValueList(entry.Name).Add(entry);
            return entry;
        }

        public IPackageDescriptorEntry Append(string name, string value)
        {
            var header = new GenericDescriptorEntry(name, value);
            return Append(header);
        }

        public void Remove(string name)
        {
            var headers = GetOrAddValueList(name);
            _headers.RemoveRange(headers);
            headers.Clear();
        }

        public void Remove(string name, string value)
        {
            var headerValue = new GenericDescriptorEntry(name, value);
            _headers.Remove(headerValue);
            GetOrAddValueList(name).Remove(headerValue);
        }

        public void Set(string name, string value)
        {
            // want to keep the order of headers, need to choose the first found one
            var allHeadersWithSameName = _headers.Where(x => x.Name.EqualsNoCase(name))
                    .ToList();

            var entryToEdit = allHeadersWithSameName.FirstOrDefault();
            var presentHeaderIndex = _headers.IndexOf(entryToEdit);
            var toRemove = allHeadersWithSameName.ToList();

            _headers.RemoveRange(toRemove);

            var headerValues = GetOrAddValueList(name);
            headerValues.Clear();

            var header = new GenericDescriptorEntry(name, value);

            if (presentHeaderIndex == -1)
                _headers.Add(header);
            else
                _headers.Insert(presentHeaderIndex, header);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<IPackageDescriptorEntry> GetEnumerator()
        {
            foreach (var val in _headers)
                yield return val;
        }

        List<IPackageDescriptorEntry> GetOrAddValueList(string name)
        {
            List<IPackageDescriptorEntry> values;
            if (!_byName.TryGetValue(name, out values))
                _byName[name] = values = new List<IPackageDescriptorEntry>();
            return values;
        }
    }
}