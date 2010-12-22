using System;
using System.Linq;
using OpenWrap.Collections;

namespace OpenWrap.Dependencies
{
    public class MultiLine<T> : NotificationCollection<T>
    {
        protected readonly DescriptorLineCollection _lines;
        readonly Func<string, T> _convertFromString;
        readonly Func<T, string> _convertToString;
        readonly string _name;


        public MultiLine(DescriptorLineCollection lines, string name, Func<T, string> convertToString, Func<string, T> convertFromString)
        {
            _lines = lines;
            _name = name;
            _convertToString = convertToString;
            _convertFromString = convertFromString;
            foreach (var line in _lines.Where(x => x.Name.EqualsNoCase(_name)))
                ParseLine(line);
        }

        protected override T HandleAdd(T item)
        {
            _lines.Append(_name, _convertToString(item));
            return item;
        }

        protected override void HandleRemove(T item)
        {
            _lines.Remove(_name, _convertToString(item));
        }

        void ParseLine(IPackageDescriptorLine line)
        {
            var converted = _convertFromString(line.Value);
            if (ReferenceEquals(converted, null) == false && converted.Equals(default(T)) == false)
                AddItemCore(converted);
        }
    }
}