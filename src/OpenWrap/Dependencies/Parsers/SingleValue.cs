using System;
using System.Linq;

namespace OpenWrap.Dependencies
{
    public class SingleValue<T>
    {
        T _currentValue;
        readonly DescriptorLineCollection _lines;
        readonly string _name;
        readonly Func<T, string> _toString;
        readonly Func<string, T> _fromString;
        readonly Func<T, bool> _isDefault;
        readonly T _defaultValue;

        public SingleValue(DescriptorLineCollection lines, string name, Func<T, string> toString, Func<string, T> fromString, T defaultValue = default(T), Func<T, bool> isDefault = null)
        {
            _lines = lines;
            _name = name;
            _toString = toString;
            _fromString = fromString;
            _defaultValue = defaultValue;
            _currentValue = defaultValue;
            _isDefault = isDefault ?? (x => ReferenceEquals(_defaultValue, null) == false && _defaultValue.Equals(x));
            var exist = lines.FirstOrDefault(x => OpenWrap.StringExtensions.EqualsNoCase(x.Name, name));
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
                    _lines.Remove(_name);
                else
                    _lines.Set(_name, stringValue);
            }
        }
    }
}