using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace OpenWrap.Build.Tasks
{
    public class GenerateRevisionNumber : Task
    {
        DateTime _from = new DateTime(2010, 01, 01);
        [Required]
        public string Version { get; set; }

        [Output]
        public string OutputVersion { get; set; }
        public override bool Execute()
        {
            var match = Regex.Match(Version.Trim(), @"^(?<version>\d+\.\d+\.\d+)(?<revision>\..*$)?");
            if (!match.Success)
            {
                OutputVersion = Version;
                return true;
            }
            var revisionMatch = match.Groups["revision"];
            if (revisionMatch.Success && revisionMatch.Value != ".*")
            {
                OutputVersion = Version;
                return true;
            }
            var now = DateTime.UtcNow;
            var seconds = now.Second;
            var days = (now - _from).TotalDays;
            var revision = ((int)days << 12) + seconds;
            OutputVersion = match.Groups["version"] + "." + revision;
            return true;
        }
    }
}
