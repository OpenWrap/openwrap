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
            OutputVersion = Version.GenerateVersionNumber();
            return true;
        }
    }
}
