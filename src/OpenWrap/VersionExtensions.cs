using System;
using System.Text.RegularExpressions;

namespace OpenWrap
{
    public static class VersionExtensions
    {
        static readonly DateTime FROM = new DateTime(2010, 01, 01);
        public static Version IgnoreRevision(this Version version)
        {
            return new Version(version.Major, version.Minor, version.Build);
        }
        public static string GenerateVersionNumber(this string version)
        {
            var match = Regex.Match(version, @"^(?<version>\d+\.\d+\.\d+)(?<revision>\..*$)?");
            if (!match.Success)
            {
                return version;
            }
            var revisionMatch = match.Groups["revision"];
            if (revisionMatch.Success && revisionMatch.Value != ".*")
            {
                return version;
            }
            var now = DateTime.UtcNow;
            var seconds = (now.Hour * 3600) + (now.Minute * 60) + now.Second;
            var days = (now - FROM).TotalDays;
            var revision = ((short)days << 17) + seconds;
            return match.Groups["version"] + "." + revision;
        }
    }
}