using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace OpenWrap.PackageModel.Parsers
{
    public class DependsParser : DependsVersionParser
    {
            static Regex _regex = new Regex(@"^\s*depends\s*:\s*(?<content>.*)$", RegexOptions.IgnoreCase);

        public static PackageDependency ParseDependsLine(string line)
        {
            var match = _regex.Match(line);
            if (!match.Success)
                return null;
            return ParseDependsValue(match.Groups["content"].Value);
        }

        public static PackageDependency ParseDependsValue(string line)
        {
            var bits = line.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            if (bits.Length < 1)
                return null;

            var versions = ParseVersions(bits.Skip(1).ToArray()).ToList();
            var tags = bits.AsEnumerable();

            if (versions.Count > 0)
                tags = tags.Skip((versions.Count * 2) + versions.Count - 1);

            tags = tags.Skip(1);

            return new PackageDependency(bits[0],
                                         versions.Count > 0 
                                            ? versions
                                            : new List<VersionVertex> { new AnyVersionVertex() },
                                         tags.ToList());
        }
    }
}