using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenWrap.Runtime
{
    public static class FrameworkVersioning
    {
        static Dictionary<string, MSBuildTargetFramework> _openWrapToTargetFrameworkVersion = new Dictionary<string, MSBuildTargetFramework>(StringComparer.OrdinalIgnoreCase)
        {
            {"net20", new MSBuildTargetFramework("v2.0")},
            {"net30", new MSBuildTargetFramework("v3.0")},
            {"net35", new MSBuildTargetFramework("v3.5")},
            {"net35cp", new MSBuildTargetFramework("v3.5", "Client")},
            {"net40", new MSBuildTargetFramework("v4.0")},
            {"net40cp", new MSBuildTargetFramework("v4.0", "Client")},
            {"sl30", new MSBuildTargetFramework("v3.0", identifier: "Silverlight")},
            {"sl40", new MSBuildTargetFramework("v4.0", identifier: "Silverlight")}
        };
        public static MSBuildTargetFramework OpenWrapToMSBuild(string openWrapProfile)
        {
            if (_openWrapToTargetFrameworkVersion.ContainsKey(openWrapProfile) == false)
                throw new ArgumentException(string.Format("The OpenWrap profile '{0}' is unknown.", openWrapProfile), "openWrapProfile");
            return _openWrapToTargetFrameworkVersion[openWrapProfile];
        
        }
    }
    public class MSBuildTargetFramework
    {
        public MSBuildTargetFramework(string version, string profile = "", string identifier = ".NETFramework")
        {
            Profile = profile;
            Version = version;
            Identifier = identifier;
        }

        public string Profile { get; private set; }
        public string Version { get; private set; }
        public string Identifier { get; private set; }
    }   
}
