using System;
using System.Linq;
using Microsoft.Build.Utilities;
using OpenFileSystem.IO;
using OpenWrap.IO;
using OpenWrap.PackageModel;
using OpenWrap.PackageModel.Serialization;

namespace OpenWrap.Build
{
    public static class BuildManagement
    {
        public static IFile TryGenerateAssemblyInfo(IFile descriptorPath, SemanticVersion providedVersion = null)
        {
            IPackageDescriptor descriptor;
            using (var str = descriptorPath.OpenRead())
                descriptor = new PackageDescriptorReader().Read(str);
            var versionFile = descriptorPath.Parent.GetFile("version");
            var version = providedVersion ?? GenerateVersion(descriptor, versionFile);
            if (version == null) return null;
            var generator = new AssemblyInfoGenerator(descriptor) { Version = version };
            var projectAsmFile = descriptorPath.Parent.GetUniqueFile("SharedAssemblyInfo.cs");
            generator.Write(projectAsmFile);
            return projectAsmFile;
        }
        // TODO: Issue with not using the correct IFile when building -from
        public static SemanticVersion GenerateVersion(IPackageDescriptor descriptor, IFile versionFile)
        {
            
            var ver = versionFile.Exists
                          ? versionFile.ReadLines().First()
                          : (descriptor.SemanticVersion != null 
                             ? descriptor.SemanticVersion.ToString()
                             : descriptor.Version.ToString());

            var lastBuildFile = versionFile.Parent.GetDirectory("wraps")
                                                  .GetDirectory("_cache")
                                                  .GetFile("_lastBuild");

            var builder = new SemanticVersionGenerator(
                ver,
                () => lastBuildFile.Exists ? lastBuildFile.ReadString() : "-1",
                lastBuildFile.WriteString);
            return builder.Version();
        }
    }
}