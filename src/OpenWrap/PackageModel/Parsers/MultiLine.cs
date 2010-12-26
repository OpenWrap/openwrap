using System;
using System.Linq;
using OpenWrap.Collections;

namespace OpenWrap.PackageModel.Parsers
{
    public class MultiLine<T> : NotificationCollection<T>
    {
        protected readonly PackageDescriptorEntryCollection _entries;
        readonly Func<string, T> _convertFromString;
        readonly Func<T, string> _convertToString;
        readonly string _name;


        public MultiLine(PackageDescriptorEntryCollection entries, string name, Func<T, string> convertToString, Func<string, T> convertFromString)
        {
            _entries = entries;
            _name = name;
            _convertToString = convertToString;
            _convertFromString = convertFromString;
            foreach (var line in _entries.Where(x => x.Name.EqualsNoCase(_name)))
                ParseLine(line);
        }

        protected override T HandleAdd(T item)
        {
            _entries.Append(_name, _convertToString(item));
            return item;
        }

        protected override void HandleRemove(T item)
        {
            _entries.Remove(_name, _convertToString(item));
        }

        void ParseLine(IPackageDescriptorEntry entry)
        {
            var converted = _convertFromString(entry.Value);
            if (ReferenceEquals(converted, null) == false && converted.Equals(default(T)) == false)
                AddItemCore(converted);
        }
    }
}