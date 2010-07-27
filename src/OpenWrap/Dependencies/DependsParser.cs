using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenWrap.Dependencies
{
    public class DependsParser : AbstractDescriptorParser
    {
        public DependsParser() : base("depends"){}
        public override void ParseContent(string content, WrapDescriptor descriptor)
        {
            var dependency = ParseDependency(content);
            if (dependency != null)
                descriptor.Dependencies.Add(dependency);
        }

        public static WrapDescriptor ParseDependsInstruction(string line)
        {
            WrapDescriptor descriptor = new WrapDescriptor();
            new DependsParser().Parse(line, descriptor);
            return descriptor;
        }
        static WrapDependency ParseDependency(string line)
        {
            var bits = line.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            if (bits.Length < 1)
                return null;

            return new WrapDependency
            {
                Name = bits[0],
                VersionVertices = ParseVersions(bits.Skip(1).ToArray()).ToList(),
                Anchored = bits.Last().Equals("anchored", StringComparison.OrdinalIgnoreCase)
            };
        }

        public static IEnumerable<VersionVertice> ParseVersions(string[] args)
        {
            // Versions are always in the format
            // comparator version ('and' comparator version)*
            if (args.Length >= 2)
            {
                var firstVersion = GetVersionVertice(args, 0);
                yield return firstVersion;
                for (int i = 0; i < (args.Length - 2) / 3; i++)
                    if (args[2+(i*3)].Equals("and", StringComparison.OrdinalIgnoreCase))
                        yield return GetVersionVertice(args, 3 + (i * 3));
            }
            else
            {
                yield return new AnyVersionVertice();
            }
        }
        private static VersionVertice GetVersionVertice(string[] strings, int offset)
        {
            var comparator = strings[offset];
            var version = new Version(strings[offset + 1]);
            switch (comparator)
            {
                case ">":
                    return new GreaterThenVersionVertice(version);
                case ">=": return new GreaterThenOrEqualVersionVertice(version);
                case "=": return new ExactVersionVertice(version);
                case "<": return new LessThanVersionVertice(version);
                default: return new AnyVersionVertice(version);
            }
        }
    }
    
}
