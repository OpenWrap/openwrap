using System;
using OpenWrap.PackageModel;

namespace OpenWrap.PackageManagement
{
    public class PackageRequest
    {
        PackageRequest()
        {
        }

        public Version ExactVersion { get; private set; }
        public bool LastVersion { get; private set; }
        public Version MaxVersion { get; private set; }
        public Version MinVersion { get; private set; }
        public string Name { get; private set; }

        public static PackageRequest Any(string packageName)
        {
            return new PackageRequest { Name = packageName };
        }

        public static PackageRequest Between(string packageName, Version minVersion, Version maxVersion)
        {
            return new PackageRequest
            {
                    Name = packageName,
                    MinVersion = minVersion,
                    MaxVersion = maxVersion
            };
        }

        public static PackageRequest Exact(string packageName, Version exactVersion)
        {
            return new PackageRequest { Name = packageName, ExactVersion = exactVersion };
        }

        public static PackageRequest Last(string packageName)
        {
            return new PackageRequest { Name = packageName, LastVersion = true };
        }
    }
}