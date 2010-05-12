using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace OpenRasta.Wrap.Dependencies
{
    public class WrapDescriptorParser
    {
        const int FILE_READ_RETRIES = 5;
        const int FILE_READ_RETRIES_WAIT = 500;
        private readonly IEnumerable<IWrapDescriptorLineParser> _lineParsers = new List<IWrapDescriptorLineParser>
                                                                                   {
                                                                                       new WrapDependencyParser()
                                                                                   };

        public WrapDescriptor ParseFile(string filePath)
        {
            int tries = 0;
            while (tries < FILE_READ_RETRIES)
            {
                try
                {
                    using (var fileStream = File.OpenRead(filePath))
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

        public WrapDescriptor ParseFile(string filePath, Stream content)
        {
            var stringReader = new StreamReader(content, true);
            string[] lines = SplitAndFold(stringReader.ReadToEnd());
            
            var descriptor = new WrapDescriptor
            {
                Name = WrapNameUtility.GetName(Path.GetFileNameWithoutExtension(filePath)),
                Version = WrapNameUtility.GetVersion(Path.GetFileNameWithoutExtension(filePath))
            };
            foreach (var line in lines)
                foreach (var parser in _lineParsers)
                    parser.Parse(line, descriptor);
            return descriptor;
        }
        static Regex _foldableLines = new Regex(@"\r\n\s+", RegexOptions.Multiline | RegexOptions.Compiled);
        string[] SplitAndFold(string content)
        {
            content = _foldableLines.Replace(content, " ");
            return content.Split(new[] { @"\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
    public class WrapNameUtility
    {
        private static Regex VersionRegex = new Regex(@"\d+\.\d+\.\d+$", RegexOptions.Compiled);
        public static string GetName(string name)
        {
            return GetVersion(name) == null ? name : name.Substring(0, name.LastIndexOf('-'));
        }
        public static Version GetVersion(string name)
        {
            var versionMAtch = VersionRegex.Match(name);
            if (versionMAtch.Success)
                return new Version(name.Substring(name.LastIndexOf('-') + 1));
            return null;
        }
    }
}