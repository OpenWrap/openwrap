using System.Collections.Generic;
using OpenFileSystem.IO.FileSystem.Local;

namespace OpenWrap.Build.BuildEngines
{
    public class MetaPackageBuilder : IPackageBuilder
    {
        readonly IEnvironment _environment;

        public MetaPackageBuilder(IEnvironment environment)
        {
            _environment = environment;
        }

        public IEnumerable<BuildResult> Build()
        {
            const string metaExportName = ".";

            var currentDirectory = _environment.CurrentDirectory;

            yield return new FileBuildResult(metaExportName, 
                                             new Path(_environment.DescriptorFile.Path.FullPath));

            // the version may not exist - it may be part of the wrapdescriptor
            // instead
            var versionFile = currentDirectory.GetFile("version");
            if (versionFile.Exists)
                yield return new FileBuildResult(metaExportName,
                                                 new Path(versionFile.Path.FullPath));
        }
    }
}