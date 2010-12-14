using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenWrap.Dependencies
{
    public class DependsParser : AbstractDescriptorParser
    {
        public DependsParser() : base("depends"){}

        protected override void ParseContent(string content, PackageDescriptor descriptor)
        {
            var dependency = ParseDependsValue(content);
            if (dependency != null)
                descriptor.Dependencies.Add(dependency);
        }
        protected override IEnumerable<string> WriteContent(PackageDescriptor descriptor)
        {
            foreach (var dependency in descriptor.Dependencies)
                yield return dependency.ToString();
        }

        public static PackageDescriptor ParseDependsInstruction(string line)
        {
            var descriptor = new PackageDescriptor();
            new DependsParser().Parse(line, descriptor);
            return descriptor;
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
            
            return new PackageDependency
            (
                    bits[0],
                    versions.Count > 0 ? versions : new List<VersionVertex>() { new AnyVersionVertex() },
                    tags.ToList()
            );
        }

        public static IEnumerable<VersionVertex> ParseVersions(string[] args)
        {
            // Versions are always in the format
            // comparator version ('and' comparator version)*
            if (args.Length >= 2)
            {
                var version = GetVersionVertice(args, 0);
                if (version == null)
                    yield break;
                yield return version;
                for (int i = 0; i < (args.Length - 2) / 3; i++)
                    if (args[2 + (i * 3)].EqualsNoCase("and"))
                        if ((version = GetVersionVertice(args, 3 + (i * 3))) != null)
                            yield return version;
                        else
                            yield break;
            }
        }
        private static VersionVertex GetVersionVertice(string[] strings, int offset)
        {
            var comparator = strings[offset];
            var version = strings[offset + 1].ToVersion();
            if (version == null)
                return null;
            switch (comparator)
            {
                case ">":
                    return new GreaterThanVersionVertex(version);
                case ">=": return new GreaterThanOrEqualVersionVertex(version);
                case "=": return new ExactVersionVertex(version);
                case "<": return new LessThanVersionVertex(version);
                default: return null;
            }
        }
    }
    
}
