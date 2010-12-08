using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using OpenWrap.Collections;

namespace OpenWrap.Dependencies
{
    public abstract class AbstractDescriptorParser : IDescriptorParser
    {
        protected string Header { get; set; }
        readonly Regex _regex;

        protected AbstractDescriptorParser(string header)
        {
            Header = header;
            _regex = new Regex(@"^\s*" + header + @"\s*:\s*(?<content>.*)$", RegexOptions.IgnoreCase);
        }

        public void Parse(string line, PackageDescriptor descriptor)
        {
            var match = _regex.Match(line);
            if (!match.Success)
                return;
            ParseContent(match.Groups["content"].Value, descriptor);
        }

        public IEnumerable<string> Write(PackageDescriptor descriptor)
        {
            var content = WriteContent(descriptor).ToList();
            if (content.Count == 0) return content;
            return content.Select(x => Header + ": " + x);
        }

        protected abstract IEnumerable<string> WriteContent(PackageDescriptor descriptor);

        protected abstract void ParseContent(string content, PackageDescriptor descriptor);
    }
    public class PackageDependencyCollection : MultiLine<PackageDependency>
    {

        public PackageDependencyCollection(DescriptorLineCollection lines)
            : base(lines, "depends", ConvertToString, ConvertFromString)
        {
        }

        static string ConvertToString(PackageDependency arg)
        {
            return arg.ToString();
        }

        static PackageDependency ConvertFromString(string arg)
        {
            return DependsParser.ParseDependsValue(arg);
        }
    }
    public class PackageNameOverrideCollection : MultiLine<PackageNameOverride>
    {
        public PackageNameOverrideCollection(DescriptorLineCollection lines)
            : base(lines, "override", ConvertToString, ConvertFromString)
        {
        }

        static string ConvertToString(PackageNameOverride arg)
        {
            return string.Format("{0} {1}", arg.OldPackage, arg.NewPackage);
        }

        static PackageNameOverride ConvertFromString(string arg)
        {
            var names = arg.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (names.Length == 2)
                return new PackageNameOverride(names[0].Trim(), names[1].Trim());
            return null;
        }
    }
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
            foreach (var line in _lines.Where(x => x.Name.EqualsNoCase(_name)))
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
            var exist = lines.FirstOrDefault(x => x.Name.EqualsNoCase(name));
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
    public class SingleStringValue : SingleValue<string>
    {
        public SingleStringValue(DescriptorLineCollection lines, string name,  string defaultValue = null)
            : base(lines, name, x => x, x => x, defaultValue: defaultValue) { }

    }
    public class SingleBoolValue : SingleValue<bool>
    {
        public SingleBoolValue(DescriptorLineCollection lines, string name, bool defaultValue)
            : base(lines, name, x => x.ToString().ToLower(), bool.Parse, defaultValue: defaultValue)
        { }
    }
    public class SingleVersionValue : SingleValue<Version>
    {
        public SingleVersionValue(DescriptorLineCollection lines, string name)
            : base(lines, name, x => x != null ? x.ToString() : null, x => x.ToVersion())
        {

        }
    }
    public class SingleDateTimeOffsetValue : SingleValue<DateTimeOffset>
    {
        public SingleDateTimeOffsetValue(DescriptorLineCollection lines, string name)
            : base(lines, name, ConvertToString, ConvertFromString)
        {
        }

        static string ConvertToString(DateTimeOffset arg)
        {
            return arg == default(DateTimeOffset) ? null : arg.ToString();
        }

        static DateTimeOffset ConvertFromString(string arg)
        {
            DateTimeOffset parseResult;
            DateTimeOffset.TryParse(arg, out parseResult);
            return parseResult;
        }
    }
}