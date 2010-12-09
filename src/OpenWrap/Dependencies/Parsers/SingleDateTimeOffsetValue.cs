using System;

namespace OpenWrap.Dependencies
{
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