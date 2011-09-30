using System.Collections.Generic;
using System.Linq;
using OpenWrap.Build;
using OpenWrap.Build.PackageBuilders;

namespace Tests.Commands.build_wrap.inputs_to_providers
{
    public class PackageBuilderWithConfig : IPackageBuilder
    {
        public static string ConfigurationValue;
        public static bool IncrementalValue;
        public static ILookup<string, string> PropertiesValue;
        public string Configuration { get { return ConfigurationValue; } set { ConfigurationValue = value; } }
        public bool Incremental { get { return IncrementalValue; } set { IncrementalValue = value; } }

        public ILookup<string, string> Properties { get { return PropertiesValue; } set { PropertiesValue = value; } }
        public IEnumerable<BuildResult> Build()
        {
            yield break;
        }
    }
}