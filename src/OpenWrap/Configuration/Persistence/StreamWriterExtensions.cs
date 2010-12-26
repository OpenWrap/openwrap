using System.IO;

namespace OpenWrap.Configuration.Persistence
{
    public static class StreamWriterExtensions
    {
        public static void WriteProperty(this StreamWriter writer, string name, object value)
        {
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