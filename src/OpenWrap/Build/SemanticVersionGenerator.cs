using System;
using System.Text.RegularExpressions;

namespace OpenWrap.Build
{
    public class SemanticVersionGenerator
    {
        const string REGEX = @"(?<major>\d+)\.(?<minor>\d+)\.(?<patch>(\d+|[*#]))" +
                             @"((\.(?<build>\d+|[*#]))|" +
                             @"(\-(?<pre>[0-9A-Za-z-\.#*]+))?" +
                             @"(\+(?<build>[0-9A-Za-z-\.#*]+))?)";
        static readonly Regex _versionBuilderRegex = new Regex(REGEX);

        readonly string _versionBuilder;
        readonly Func<string> _incrementalReader;
        readonly Action<string> _incrementalWriter;

        public SemanticVersionGenerator(string versionBuilder, Func<string> incrementalReader, Action<string> incrementalWriter)
        {
            _versionBuilder = versionBuilder;
            _incrementalReader = incrementalReader;
            _incrementalWriter = incrementalWriter;
        }

        public SemanticVersion Version()
        {
            var match = _versionBuilderRegex.Match(_versionBuilder);
            if (!match.Success) 
                throw new InvalidOperationException(string.Format("Invalid version specifier '{0}'.", _versionBuilder));
            var major = int.Parse(match.Groups["major"].Value);
            var minor = int.Parse(match.Groups["minor"].Value);
            var patch = match.Groups["patch"].Success ? match.Groups["patch"].Value : null;
            int patchNum;
            int timeBased = GenerateTimeBasedRevision();
            int? incremental = null;
            if (!int.TryParse(patch, out patchNum))
            {
                if (patch == "*") patchNum = timeBased;
                else if (patch == "#")
                {
                    incremental = patchNum = int.Parse(_incrementalReader())+1;
                    
                }
                else
                    throw new InvalidOperationException(string.Format("Unknown patch identifier '{0}'.", patch));
            }
            var pre = match.Groups["pre"].Success ? match.Groups["pre"].Value : null;
            var build = match.Groups["build"].Success ? match.Groups["build"].Value : null;
            if (build != null)
            {
                build = ReplaceTimeBased(build, timeBased);
                if (build.Contains("#"))
                {
                    if (incremental == null)
                        incremental = int.Parse(_incrementalReader())+1;
                    build = build.Replace("#", incremental.ToString());
                }
            }
            if (incremental != null)
                _incrementalWriter(incremental.ToString());
            return new SemanticVersion(major, minor, patchNum, pre, build);
        }

        string ReplaceTimeBased(string ver, int build)
        {
            return ver.Replace("*", build.ToString());
        }

        static readonly DateTime FROM = new DateTime(2010, 01, 01);

        static int GenerateTimeBasedRevision()
        {
            var now = DateTime.UtcNow;
            var seconds = (now.Hour * 3600) + (now.Minute * 60) + now.Second;
            var days = (now - FROM).TotalDays;
            var revision = ((short)days << 17) + seconds;
            return revision;
        }
    }
}