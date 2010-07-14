using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using OpenFileSystem.IO;

namespace OpenWrap.Dependencies
{
    public class WrapDescriptorParser
    {
        const int FILE_READ_RETRIES = 5;
        const int FILE_READ_RETRIES_WAIT = 500;
        
        static readonly Regex _foldableLines = new Regex(@"\r\n\s+", RegexOptions.Multiline | RegexOptions.Compiled);

        readonly IEnumerable<IWrapDescriptorLineParser> _lineParsers = new List<IWrapDescriptorLineParser>
        {
            new WrapDependencyParser(),
            new WrapDescriptionParser()
        };

        public WrapDescriptor ParseFile(IFile filePath)
        {
            int tries = 0;
            while (tries < FILE_READ_RETRIES)
            {
                try
                {
                    using (var fileStream = filePath.OpenRead())
                    {
                        return ParseFile(filePath, fileStream);
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

        public WrapDescriptor ParseFile(IFile filePath, Stream content)
        {
            var stringReader = new StreamReader(content, true);
            var lines = SplitAndFold(stringReader.ReadToEnd());

            var descriptor = new WrapDescriptor
            {
                Name = WrapNameUtility.GetName(filePath.NameWithoutExtension),
                Version = WrapNameUtility.GetVersion(filePath.NameWithoutExtension),
                File = filePath
            };
            foreach (var line in lines)
                foreach (var parser in _lineParsers)
                    parser.Parse(line, descriptor);
            return descriptor;
        }

        string[] SplitAndFold(string content)
        {
            content = _foldableLines.Replace(content, " ");
            return content.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}