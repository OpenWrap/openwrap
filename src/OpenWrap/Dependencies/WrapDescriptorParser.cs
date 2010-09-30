using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using OpenFileSystem.IO;
using OpenWrap.Dependencies.Parsers;

namespace OpenWrap.Dependencies
{
    public class WrapDescriptorParser
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
            new UseProjectRepositoryParser()
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
        // Version comes from the version file first, then the version: header in the descriptor, then from the filename if there is one
        public WrapDescriptor ParseFile(IFile filePath, Stream content)
        {
            var stringReader = new StreamReader(content, true);
            var lines = stringReader.ReadToEnd().GetUnfoldedLines();
            IFile versionFile;
            var descriptor = new WrapDescriptor
            {
                Name = PackageNameUtility.GetName(filePath.NameWithoutExtension),
                Version = PackageNameUtility.GetVersion(filePath.NameWithoutExtension),
                File = filePath
            };
            foreach (var line in lines)
                foreach (var parser in _lineParsers)
                    parser.Parse(line, descriptor);
            
            if (filePath.Parent != null && (versionFile = filePath.Parent.GetFile("version")).Exists)
            {

                using(var versionStream = versionFile.OpenRead())
                {
                    try
                    {
                        descriptor.Version = new Version(versionStream.ReadString(Encoding.UTF8));
                        descriptor.IsVersionInDescriptor = false;
                    }
                    catch{}
                }
            }
            return descriptor;
        }
        public void SaveDescriptor(WrapDescriptor descriptor)
        {
            using (var descriptorStream = descriptor.File.OpenWrite())
            using (var streamWriter = new StreamWriter(descriptorStream, Encoding.UTF8))
            {
                var lines = from parser in _lineParsers
                            from parserLine in parser.Write(descriptor)
                            select parserLine;
                
                streamWriter.Write(string.Join("\r\n", lines.ToArray()));
            }
        }
    }
    public static class StringExtensions
    {
        static readonly Regex _foldableLines = new Regex(@"\r\n[\f\t\v\x85\p{Z}]+", RegexOptions.Multiline | RegexOptions.Compiled);
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