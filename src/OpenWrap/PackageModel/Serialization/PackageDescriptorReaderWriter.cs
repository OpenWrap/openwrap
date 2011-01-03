using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using OpenFileSystem.IO;

namespace OpenWrap.PackageModel.Serialization
{
    public class PackageDescriptorReaderWriter
    {
        const int FILE_READ_RETRIES = 5;
        const int FILE_READ_RETRIES_WAIT = 20;

        // TODO: Read-retry should be part of an extension method that can be reused for reading the index in indexed folder repositories.

        static readonly Regex _lineParser = new Regex(@"^\s*(?<fieldname>[0-9a-zA-Z\-\.]*?)\s*:\s*(?<fieldvalue>.*?)\s*$", RegexOptions.Compiled);

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

        public PackageDescriptor Read(Stream content)
        {
            var stringReader = new StreamReader(content, true);
            var lines = stringReader.ReadToEnd().GetUnfoldedLines();

            return new PackageDescriptor(lines.Select(ParseLine));
        }

        public void Write(IEnumerable<IPackageDescriptorEntry> descriptor, Stream descriptorStream)
        {
            var streamWriter = new StreamWriter(descriptorStream, Encoding.UTF8);
            foreach (var line in descriptor)
            {
                streamWriter.Write(line.Name + ": " + line.Value + "\r\n");
            }
            streamWriter.Flush();
        }

        IPackageDescriptorEntry ParseLine(string line)
        {
            var match = _lineParser.Match(line);
            return !match.Success
                           ? null
                           : new GenericDescriptorEntry(
                                     match.Groups["fieldname"].Value,
                                     match.Groups["fieldvalue"].Value);
        }
    }
}