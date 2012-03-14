using System;
using System.Collections.Generic;

namespace OpenWrap.PackageModel.Parsers
{
    public class DependsVersionParser
    {
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
                    if (args[2 + (i * 3)].Equals("and", StringComparison.OrdinalIgnoreCase))
                        if ((version = GetVersionVertice(args, 3 + (i * 3))) != null)
                            yield return version;
                        else
                            yield break;
            }
        }

        static VersionVertex GetVersionVertice(string[] strings, int offset)
        {
            var comparator = strings[offset];
            var version = SemanticVersion.TryParseExact(strings[offset + 1]);
            if (version == null)
                return null;
            switch (comparator)
            {
                case ">":
                    return new GreaterThanVersionVertex(version);
                case ">=":
                    return new GreaterThanOrEqualVersionVertex(version);
                case "=":
                    return new EqualVersionVertex(version);
                case "<":
                    return new LessThanVersionVertex(version);
                case "<=":
                    return new LessThanOrEqualVersionVertex(version);
                case "≡":
                    return new AbsolutelyEqualVersionVertex(version);
                case "~>":
                    return new PessimisticGreaterVersionVertex(version);
                default:
                    return null;
            }
        }
    }
}