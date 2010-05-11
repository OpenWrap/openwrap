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

        public WrapDescriptor Parse(string filePath)
        {
            int tries = 0;
            while (tries < FILE_READ_RETRIES)
            {
                try
                {
                    var lines = File.ReadAllLines(filePath);

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
                catch(IOException)
                {
                    tries++;
                    Thread.Sleep(FILE_READ_RETRIES_WAIT);
                    continue;
                }
            }
            return null;
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
            if(versionMAtch.Success)
                return new Version(name.Substring(name.LastIndexOf('-')+1));
            return null;
        }
    }
}