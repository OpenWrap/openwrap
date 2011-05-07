using System;
using System.Collections;
using System.IO;
using System.Linq;
using OpenWrap.Collections;

namespace OpenWrap.Configuration.Persistence
{
    public static class StreamWriterExtensions
    {
        public static void WriteProperty(this StreamWriter writer, string name, object value)
        {
            var enumerable = value as IEnumerable;
            if (value == null || ((value is string) == false && enumerable != null && enumerable.GetEnumerator().MoveNext() == false)) return;
            if (enumerable != null && (value is string) == false)
                writer.WriteLine("{0}: {1}", name, enumerable.Select(x => x.ToString()).JoinString(", "));
            else
                writer.WriteLine("{0}: {1}", name, value);
        }

        public static void WriteSection(this StreamWriter writer, string sectionType, string sectionName)
        {
            writer.WriteLine();
            if (string.IsNullOrEmpty(sectionName))
                writer.WriteLine("[{0}]", sectionType.ToLower());
            else
                writer.WriteLine("[{0} {1}]", sectionType.ToLower(), sectionName);
        }
    }
}