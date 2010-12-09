using System;
using System.Linq;
using OpenWrap.Collections;

namespace OpenWrap.Dependencies
{
    public class MultiLine<T> : NotificationCollection<T>
    {
        protected readonly DescriptorLineCollection _lines;
        string _name;
        readonly Func<T, string> _convertToString;
        readonly Func<string, T> _convertFromString;


        public MultiLine(DescriptorLineCollection lines, string name, Func<T, string> convertToString, Func<string, T> convertFromString)
        {
            _lines = lines;
            _name = name;
            _convertToString = convertToString;
            _convertFromString = convertFromString;
            foreach (var line in _lines.Where(x => OpenWrap.StringExtensions.EqualsNoCase(x.Name, _name)))
                ParseLine(line);
        }

        void ParseLine(IPackageDescriptorLine line)
        {
            var converted = _convertFromString(line.Value);
            if (ReferenceEquals(converted, null) == false && converted.Equals(default(T)) == false)
                AddItemCore(converted);
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
    }
}