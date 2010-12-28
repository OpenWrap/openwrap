using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using OpenFileSystem.IO;

namespace OpenWrap.Build
{
    public class DefaultFileBuildResultParser : IFileBuildResultParser
    {
        static readonly Regex _buildInstructionRegex = new Regex(@"\[built\((?<export>.+)\s*(\,\s*('|"")\s*(?<fileSpec>.*)\s*('|"")(\,\s*(?<duplicates>(true|false)))?)\)\]");

        public IEnumerable<FileBuildResult> Parse(string line)
        {
            var instructionMatch = _buildInstructionRegex.Match(line);
            if (instructionMatch.Success)
            {
                var fileSpec = instructionMatch.Groups["fileSpec"];
                var exportName = instructionMatch.Groups["export"];
                var allowDuplicate = instructionMatch.Groups["duplicates"];
                if (fileSpec.Success && exportName.Success)
                {
                    return (
                                   from x in fileSpec.Value.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries)
                                   let allowDuplicatesValue = allowDuplicate.Success ? bool.Parse(allowDuplicate.Value) : true
                                   select new FileBuildResult(exportName.Value.Trim(), new Path(x), allowDuplicatesValue)
                           )
                            .ToList();
                }
            }
            return Enumerable.Empty<FileBuildResult>();
        }
    }
}