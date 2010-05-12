using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenRasta.Wrap.Dependencies
{
    public interface IWrapDescriptorLineParser
    {
        void Parse(string line, WrapDescriptor descriptor);
    }
    public class WrapDependencyParser : IWrapDescriptorLineParser
    {
        public void Parse(string line, WrapDescriptor descriptor)
        {
            var bits = line.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            if (bits == null || bits.Length < 2 || string.Compare(bits[0], "depends", StringComparison.OrdinalIgnoreCase) != 0)
                return;

            var packageName = bits[1];

            // Versions are always in the format
            // comparator version ('and' comparator version)*

            //List<VersionVertice> vertices = new List<VersionVertice>();
            //if (bits.Length - 2 >= 2)
            //{
            //    vertices.Add(GetVersionVertice(bits, 2));
            //    for (int i = 0; i < (bits.Length - 4) / 3; i++)
            //        if (string.Compare(bits[4], "and", StringComparison.OrdinalIgnoreCase) == 0)
            //            vertices.Add(GetVersionVertice(bits, 5 + (i * 3)));
            //}
            //else
            //{
            //    vertices.Add(new AnyVersionVertice(null));
            //}
            descriptor.Dependencies.Add(new WrapDependency
                       {
                           Name = packageName,
                           VersionVertices = Parse(bits.Skip(2).ToArray()).ToList()
                       });
        }
        public static IEnumerable<VersionVertice> Parse(string[] args)
        {

            if (args.Length >= 2)
            {
                yield return GetVersionVertice(args, 0);
                for (int i = 0; i < (args.Length - 2) / 3; i++)
                    if (args[2+(i*3)].Equals("and", StringComparison.OrdinalIgnoreCase))
                        yield return GetVersionVertice(args, 3 + (i * 3));
            }
            else
            {
                yield return new AnyVersionVertice(null);
            }
        }
        private static VersionVertice GetVersionVertice(string[] strings, int offset)
        {
            var comparator = strings[offset];
            var version = strings[offset + 1];
            switch (comparator)
            {
                case ">=": return new AtLeastVersionVertice(new Version(version));
                case "=": return new ExactVersionVertice(new Version(version));
                case "<": return new LessThanVersionVertice(new Version(version));
                default: return new AnyVersionVertice(new Version(version));
            }
        }
    }
    
}
