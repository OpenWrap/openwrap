using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using OpenFileSystem.IO;
using OpenWrap.Dependencies.Parsers;

namespace OpenWrap
{
}

namespace OpenWrap.Dependencies
{
    public class PackageDescriptorReaderWriter
    {
        const int FILE_READ_RETRIES = 5;
        const int FILE_READ_RETRIES_WAIT = 500;


        readonly IEnumerable<IDescriptorParser> _lineParsers = new List<IDescriptorParser>
        {
            new DependsParser(),
            new DescriptionParser(),
            new OverrideParser(),
            new AnchorParser(),
            new BuildParser(),

            new UseProjectRepositoryParser(),
            new VersionParser()
        };

        public PackageDescriptor ParseFile(IFile filePath)
        {
            int tries = 0;
            while (tries < FILE_READ_RETRIES)
            {
                try
                {
                    using (var fileStream = filePath.OpenRead())
                    {
                        var descriptor = ParseFile(fileStream);
                        if (descriptor.Name == null)
                            descriptor.Name = PackageNameUtility.GetName(filePath.NameWithoutExtension);
                        return descriptor;
                    }
                }
                catch (IOException)
                {
                    tries++;
                    Thread.Sleep(FILE_READ_RETRIES_WAIT);
                    continue;
                }
            }
            return null;
        }
        public PackageDescriptor ParseFile(Stream content)
        {
            var stringReader = new StreamReader(content, true);
            var lines = stringReader.ReadToEnd().GetUnfoldedLines();

            var descriptor = new PackageDescriptor();
            foreach (var line in lines)
                foreach (var parser in _lineParsers)
                    parser.Parse(line, descriptor);

            return descriptor;
        }
        public void SaveDescriptor(PackageDescriptor descriptor, Stream descriptorStream)
        {
            var streamWriter = new StreamWriter(descriptorStream, Encoding.UTF8);
            var lines = from parser in _lineParsers
                        from parserLine in parser.Write(descriptor)
                        select parserLine;

            var content = lines.Join("\r\n");
            streamWriter.Write(content);
            streamWriter.Flush();
        }
    }

    public static class StringExtensions
    {
        static readonly Regex _foldableLines = new Regex(@"\r\n[\f\t\v\x85\p{Z}]+", RegexOptions.Multiline | RegexOptions.Compiled);
        public static string Join(this IEnumerable<string> strings, string separator)
        {
            return string.Join(separator, strings.ToArray());
        }
        public static string[] GetUnfoldedLines(this string content)
        {
            content = _foldableLines.Replace(content, " ");
            return content.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .Where(x => x != string.Empty)
                    .ToArray();
        }
    }
}
