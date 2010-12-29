using System;
using System.Collections.Generic;
using System.Linq;
using OpenFileSystem.IO;
using OpenWrap.Build;
using OpenWrap.Build.PackageBuilders;

namespace OpenWrap.Commands.Wrap
{
    public class FilePackageBuilder : IPackageBuilder
    {
        public IEnumerable<string> File { get; set; }

        public FilePackageBuilder()
        {
            File = Enumerable.Empty<string>();

        }
        public IEnumerable<BuildResult> Build()
        {
            return from fileElement in File
                   let segments = fileElement.Split(new[] { "->" }, StringSplitOptions.RemoveEmptyEntries)
                   where segments.Length >= 2
                   select (BuildResult)new FileBuildResult(segments[0].Trim(), new Path(segments[1].Trim()), true);
        }
    }
}