using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using OpenWrap.Collections;

namespace OpenWrap.Runtime
{
    public class TargetFramework : IEquatable<TargetFramework>
    {
        TargetFramework(string version, string profile = "", string identifier = ".NETFramework")
        {
            Profile = profile;
            Version = version;
            Identifier = identifier;
        }


        /// <summary>
        /// TargetFrameworkProfile
        /// </summary>
        public string Profile { get; private set; }

        /// <summary>
        /// TargetFrameworkVersion
        /// </summary>
        public string Version { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public string Identifier { get; private set; }

        static Dictionary<string, TargetFramework> _openWrapToTargetFrameworkVersion = new Dictionary<string, TargetFramework>(StringComparer.OrdinalIgnoreCase)
        {
            {"net20", new TargetFramework("v2.0")},
            {"net30", new TargetFramework("v3.0")},
            {"net35", new TargetFramework("v3.5")},
            {"net35cp", new TargetFramework("v3.5", "Client")},
            {"net40", new TargetFramework("v4.0")},
            {"net40cp", new TargetFramework("v4.0", "Client")},
            {"sl30", new TargetFramework("v3.0", identifier: "Silverlight")},
            {"sl40", new TargetFramework("v4.0", identifier: "Silverlight")},
            {"wp70", new TargetFramework("v4.0", "WindowsPhone", "Silverlight")},
        };

        static IEnumerable<KeyValuePair<string, string>> _identifierAliases = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string,string>(".NETFramework", "NET"),
            new KeyValuePair<string,string>(".NETFramework", ".NET"),
            new KeyValuePair<string,string>(".NETFramework", "NETFramework"),
            new KeyValuePair<string,string>(".NETFramework", ".NETFramework"),
            new KeyValuePair<string,string>("Silverlight", "Silverlight"),
            new KeyValuePair<string,string>("Silverlight", "sl")
        };
        public static TargetFramework New(string version, string profile = "", string identifier = ".NETFramework")
        {
            return _openWrapToTargetFrameworkVersion
                .Where(x => x.Value.Version == version && x.Value.Identifier == identifier && x.Value.Profile == profile)
                .Select(x => x.Value)
                .FirstOrDefault();
        }
        public static TargetFramework ParseOpenWrapIdentifier(string openwrapIdentifier)
        {
            if (openwrapIdentifier == null) return null;
            if (_openWrapToTargetFrameworkVersion.ContainsKey(openwrapIdentifier) == false)
                throw new ArgumentException(String.Format("The OpenWrap profile '{0}' is unknown.", openwrapIdentifier), "openwrapIdentifier");
            return _openWrapToTargetFrameworkVersion[openwrapIdentifier];
        }
        public static TargetFramework ParseDTEIdentifier(uint targetFramework, string frameworkMoniker)
        {
            // NOTE: Parsing code inspired form http://stackoverflow.com/questions/2956415/change-the-target-framework-for-all-my-projects-in-a-visual-studio-2010-solution
            
            if (frameworkMoniker != null) return ParseFrameworkMoniker(frameworkMoniker);
            // We're in vs2008, use the hacky ways.
            switch(targetFramework)
            {
                case 0x20000: return new TargetFramework("v2.0");
                case 0x30000: return new TargetFramework("v3.0");
                case 0x30005: return new TargetFramework("v3.5");
                case 0x40000: return new TargetFramework("v4.0");
            }
            return null;
        }

        static Regex _frameworkMoniker = new Regex(@"^(?<identifier>.*)\s*,\s*Version=(?<version>v\d+\.\d+(\.\d+(\.\d+)?)?)(\s*,\s*Profile=(?<profile>.*))?$");
        static TargetFramework ParseFrameworkMoniker(string frameworkMoniker)
        {
            var match = _frameworkMoniker.Match(frameworkMoniker);
            if (match.Success == false) return null;
            var identifier = match.Groups["identifier"].Value;
            var version = match.Groups["version"].Value;
            var profile = match.Groups["profile"].Success ? match.Groups["profile"].Value : string.Empty;
            identifier =  NormalizeIdentifier(identifier);
            return TargetFramework.New(version, profile, identifier);
        }

        static string NormalizeIdentifier(string identifier)
        {
            if (_openWrapToTargetFrameworkVersion.Values.None(x=>x.Identifier == identifier))
            {
                identifier = _identifierAliases.Where(x => x.Value.Equals(identifier, StringComparison.OrdinalIgnoreCase))
                    .Select(x=>x.Key)
                    .FirstOrDefault();
                if (identifier == null) return null;
            }
            return identifier;
        }

        public string ToFrameworkMoniker()
        {
            var sb = new StringBuilder();
            sb.Append(Identifier).Append(",Version=").Append(Version);
            if (Profile != string.Empty)
                sb.Append(",Profile=").Append(Profile);
            return sb.ToString();
        }
        public string ToOpenWrapMoniker()
        {
            return _openWrapToTargetFrameworkVersion.Where(x => x.Value.Equals(this)).Select(x=>x.Key).FirstOrDefault();
        }

        public bool Equals(TargetFramework other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Profile, Profile) && Equals(other.Version, Version) && Equals(other.Identifier, Identifier);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(TargetFramework)) return false;
            return Equals((TargetFramework)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = (Profile != null ? Profile.GetHashCode() : 0);
                result = (result * 397) ^ (Version != null ? Version.GetHashCode() : 0);
                result = (result * 397) ^ (Identifier != null ? Identifier.GetHashCode() : 0);
                return result;
            }
        }
        public override string ToString()
        {
            return ToOpenWrapMoniker();
        }
    }
}
