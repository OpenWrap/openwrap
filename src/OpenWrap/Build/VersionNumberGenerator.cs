using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using OpenFileSystem.IO;
using OpenWrap.Dependencies;

namespace OpenWrap.Build
{
    public static class VersionExtensions
    {
        static readonly DateTime FROM = new DateTime(2010, 01, 01);

        public static string GenerateVersionNumber(this string version)
        {
            var match = Regex.Match(version.ToString(), @"^(?<version>\d+\.\d+\.\d+)(?<revision>\..*$)?");
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
            var revision = ((short)days << 16) + seconds;
            return match.Groups["version"] + "." + revision;
        }
    }
}
