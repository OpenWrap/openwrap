﻿using System;

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
            return DateTimeOffset.TryParse(arg, out parseResult) ? parseResult : default(DateTimeOffset);
        }

        static string ConvertToString(DateTimeOffset arg)
        {
            return arg == default(DateTimeOffset) ? null : arg.ToString();
        }

        public static SingleDateTimeOffsetValue New(PackageDescriptorEntryCollection entries, string name, DateTimeOffset defaultVal)
        {
            return new SingleDateTimeOffsetValue(entries, name);
        }
    }
}