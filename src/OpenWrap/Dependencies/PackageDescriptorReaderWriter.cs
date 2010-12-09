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
        const int FILE_READ_RETRIES_WAIT = 20;


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

        public PackageDescriptor Read(IFile filePath)
        {
            if (!filePath.Exists)
                return null;
            IOException ioException = null;
            int tries = 0;
            while (tries < FILE_READ_RETRIES)
            {
                try
                {
                    using (var fileStream = filePath.OpenRead())
                    {
                        var descriptor = Read(fileStream);
                        if (descriptor.Name == null)
                            descriptor.Name = PackageNameUtility.GetName(filePath.NameWithoutExtension);
                        return descriptor;
                    }
                }
                catch (IOException ex)
                {
                    ioException = ex;
                    tries++;
                    Thread.Sleep(FILE_READ_RETRIES_WAIT);
                    continue;
                }
            }
            throw ioException;
        }

        static Regex _lineParser = new Regex(@"^\s*(?<fieldname>[0-9a-zA-Z\-\.]*)\s*:\s*(?<fieldvalue>.*)\s*$", RegexOptions.Compiled);

        public PackageDescriptor Read(Stream content)
        {
            var stringReader = new StreamReader(content, true);
            var lines = stringReader.ReadToEnd().GetUnfoldedLines();

            return new PackageDescriptor(lines.Select(ParseLine));
        }

        IPackageDescriptorLine ParseLine(string line)
        {
            var match = _lineParser.Match(line);
            return !match.Success
                           ? null
                           : new GenericDescriptorLine(
                                     match.Groups["fieldname"].Value,
                                     match.Groups["fieldvalue"].Value);
        }

        public void Write(PackageDescriptor descriptor, Stream descriptorStream)
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
}
