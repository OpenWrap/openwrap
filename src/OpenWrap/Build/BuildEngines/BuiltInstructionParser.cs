using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using OpenFileSystem.IO.FileSystem.Local;

namespace OpenWrap.Build.BuildEngines
{
    public class BuiltInstructionParser
    {
        static readonly Regex _buildInstructionRegex = new Regex(@"\[built\((<?export>.+)(\,(?<fileSpec>.*))\)\]");

        public IEnumerable<FileBuildResult> Parse(string line)
        {
            var instructionMatch = _buildInstructionRegex.Match(line);
            if (instructionMatch.Success)
            {
                var fileSpec = instructionMatch.Groups["fileSpec"];
                var exportName = instructionMatch.Groups["export"];
                if (fileSpec.Success && exportName.Success)
                {
                    return (
                                   from x in fileSpec.Value.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries)
                                   select new FileBuildResult(exportName.Value.Trim(), new LocalPath(x))
                           )
                            .ToList();
                }
            }
            return Enumerable.Empty<FileBuildResult>();
        }
    }
}