using System;

namespace OpenWrap.PackageModel.Parsers
{
    public class SingleDateTimeOffsetValue : SingleValue<DateTimeOffset>
    {
        public SingleDateTimeOffsetValue(PackageDescriptorEntryCollection entries, string name)
                : base(entries, name, ConvertToString, ConvertFromString)
        {
        }

        static DateTimeOffset ConvertFromString(string arg)
        {
            DateTimeOffset parseResult;
            DateTimeOffset.TryParse(arg, out parseResult);
            return parseResult;
        }

        static string ConvertToString(DateTimeOffset arg)
        {
            return arg == default(DateTimeOffset) ? null : arg.ToString();
        }
    }
}