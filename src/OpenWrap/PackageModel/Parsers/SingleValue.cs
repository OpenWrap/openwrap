using System;
using System.Linq;

namespace OpenWrap.PackageModel.Parsers
{
    public class SingleValue<T>
    {
        readonly T _defaultValue;
        readonly Func<string, T> _fromString;
        readonly Func<T, bool> _isDefault;
        readonly PackageDescriptorEntryCollection _entries;
        readonly string _name;
        readonly Func<T, string> _toString;
        T _currentValue;

        public SingleValue(PackageDescriptorEntryCollection entries, string name, Func<T, string> toString, Func<string, T> fromString, T defaultValue = default(T), Func<T, bool> isDefault = null)
        {
            _entries = entries;
            _name = name;
            _toString = toString;
            _fromString = fromString;
            _defaultValue = defaultValue;
            _currentValue = defaultValue;
            _isDefault = isDefault ?? (x => ReferenceEquals(_defaultValue, null) == false && _defaultValue.Equals(x));
            var exist = entries.FirstOrDefault(x => x.Name.EqualsNoCase(name));
            _currentValue = exist != null ? fromString(exist.Value) : defaultValue;
        }

        public T Value
        {
            get { return _currentValue; }
            set
            {
                _currentValue = value;
                string stringValue;

                if (_isDefault(value) || (stringValue = _toString(value)) == null)
                    _entries.Remove(_name);
                else
                    _entries.Set(_name, stringValue);
            }
        }
    }
}