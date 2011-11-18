using System;
using System.Text.RegularExpressions;
using OpenFileSystem.IO;
using OpenWrap.IO;

namespace OpenWrap
{
    public static class VersionExtensions
    {
        static readonly DateTime FROM = new DateTime(2010, 01, 01);
        public static Version IgnoreRevision(this Version version)
        {
            return version.Build >= 0 ? new Version(version.Major, version.Minor, version.Build) : new Version(version.Major, version.Minor);
        }
        public static string GenerateVersionNumber(this string versionContent, IDirectory buildCacheDirectory = null)
        {
            var match = Regex.Match(versionContent, @"^(?<version>\d+\.\d+\.\d+)(?<revision>\..*$)?");
            if (!match.Success)
                return null;

            var revisionMatch = match.Groups["revision"];
            if (!revisionMatch.Success)
                return match.Groups["version"].Value;

            if (revisionMatch.Value == ".+")
                return match.Groups["version"].Value + "." + GenerateIncrementalRevision(buildCacheDirectory);
            if (revisionMatch.Value == ".*")
                return match.Groups["version"].Value + "." + GenerateTimeBasedRevision();
            throw new InvalidOperationException("Version not recognized.");
        }

        static int GenerateIncrementalRevision(IDirectory versionFile)
        {
            var lastBuild= versionFile.GetFile("_lastVersion");
            int count = 0;
            if (lastBuild.Exists)
                count = int.Parse(lastBuild.ReadString())+1;

            lastBuild.WriteString(count.ToString());
            return count;
        }


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