using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using OpenFileSystem.IO;
using OpenWrap.Build;
using OpenWrap.Build.BuildEngines;
using OpenWrap.Dependencies;
using OpenWrap.Repositories;
using OpenWrap.Services;

namespace OpenWrap.Commands.Wrap
{
    [Command(Noun = "wrap", Verb = "build", Description = "Builds all projects and creates a wrap package.")]
    public class BuildWrapCommand : AbstractCommand
    {
        IDirectory _destinationPath;

        [CommandInput]
        public string Name { get; set; }

        [CommandInput]
        public string Path { get; set; }

        [CommandInput]
        public bool Quiet { get; set; }

        protected IEnvironment Environment
        {
            get { return Services.Services.GetService<IEnvironment>(); }
        }

        protected IFileSystem FileSystem
        {
            get { return Services.Services.GetService<IFileSystem>(); }
        }

        public override IEnumerable<ICommandOutput> Execute()
        {
            return Either(NoDescriptorFound)
                    .Or(VerifyPath)
                    .Or(Build());
        }

        IEnumerable<ICommandOutput> Build()
        {
            var packageDescriptorForEmbedding = new PackageDescriptor(Environment.Descriptor);
            var packageName = Name 
                ?? Environment.Descriptor.Name;
            var destinationPath = _destinationPath ?? Environment.CurrentDirectory;

            var packageBuilder = CreateBuilder(Environment.Descriptor.BuildCommand);

            var buildFiles = new List<FileBuildResult>();
            Action<FileBuildResult> onFound = buildFiles.Add;

            foreach (var m in ProcessBuildResults(packageBuilder, onFound)) yield return m;

            var generatedVersion = GetPackageVersion(buildFiles, packageDescriptorForEmbedding);

            if (generatedVersion == null)
                yield return new GenericError("Could not build package, no version found.");

            packageDescriptorForEmbedding.Version = generatedVersion;
            packageDescriptorForEmbedding.Name = packageName;

            foreach (var file in buildFiles)
                yield return new GenericMessage(string.Format("Copying: {0} - {1}", file.ExportName, file.Path));

            var packageFilePath = destinationPath.GetFile(
                PackageNameUtility.PacakgeFileName(packageName, generatedVersion.ToString()));

            var packageContent = GeneratePackageContent(buildFiles).Concat(
                                    GenerateVersionFile(generatedVersion),
                                    GenerateDescriptorFile(packageDescriptorForEmbedding)
                                 );

            PackageBuilder.NewFromFiles(packageFilePath, packageContent);
            yield return new GenericMessage(string.Format("Package built at '{0}'.", packageFilePath));

        }

        IEnumerable<PackageContent> GeneratePackageContent(IEnumerable<FileBuildResult> buildFiles)
        {
            return (
                           from fileDescriptor in buildFiles
                           let file = FileSystem.GetFile(fileDescriptor.Path.FullPath)
                           where file.Exists
                           select new PackageContent
                           {
                               FileName = file.Name,
                               RelativePath = fileDescriptor.ExportName,
                               Stream = () => file.OpenRead()
                           }
                   );
        }

        IEnumerable<ICommandOutput> ProcessBuildResults(IPackageBuilder packageBuilder, Action<FileBuildResult> onFound)
        {
            foreach (var t in packageBuilder.Build())
            {
                if (t is TextBuildResult && !Quiet)
                    yield return new GenericMessage(t.Message);
                else if (t is FileBuildResult)
                {
                    var buildResult = (FileBuildResult)t;
                    onFound(buildResult);
                    yield return new GenericMessage(string.Format("Output found - {0}: '{1}'", buildResult.ExportName, buildResult.Path));
                }
                else if (t is ErrorBuildResult)
                {
                    yield return new GenericError(t.Message);
                    yield break;
                }
            }
        }

        PackageContent GenerateDescriptorFile(PackageDescriptor descriptor)
        {
            var descriptorStream = new MemoryStream();
            new PackageDescriptorReaderWriter().Write(descriptor, descriptorStream);
            return new PackageContent
            {
                FileName = descriptor.Name + ".wrapdesc",
                RelativePath = ".",
                Stream = descriptorStream.ResetOnRead(),
                Size = descriptorStream.Length
            };
        }

        PackageContent GenerateVersionFile(Version generatedVersion)
        {
            var versionStream = generatedVersion.ToString().ToUTF8Stream();
            return new PackageContent
            {
                FileName = "version",
                RelativePath = ".",
                Stream = versionStream.ResetOnRead(),
                Size = versionStream.Length
            };
        }

        Version GetPackageVersion(List<FileBuildResult> buildFiles, PackageDescriptor packageDescriptorForEmbedding)
        {
            // gets the package version from (in this order):
            // 1. 'version' file generated by the build
            // 2. 'version' file living alongside the .wrapdesc file
            // 3. 'version:' header in wrap descriptor

            return new DefaultPackageInfo(string.Empty, GetVersionFromVersionFiles(buildFiles), packageDescriptorForEmbedding).Version;
        }

        bool IsVersion(FileBuildResult build)
        {
            return build.ExportName == "." && build.FileName.Equals("version", StringComparison.OrdinalIgnoreCase);
        }
        Version GetVersionFromVersionFiles(List<FileBuildResult> buildFiles)
        {

            var generatedVersion = (from buildContent in buildFiles
                                    where IsVersion(buildContent)
                                    let file = FileSystem.GetFile(buildContent.Path.FullPath)
                                    where file.Exists
                                    from line in file.ReadLines()
                                    let version = line.GenerateVersionNumber().ToVersion()
                                    where version != null
                                    select new
                                    {
                                        version,
                                        buildContent
                                    }).FirstOrDefault();
            if (generatedVersion != null)
            {
                buildFiles.Remove(generatedVersion.buildContent);
                return generatedVersion.version;
            }
            return Environment.DescriptorFile.Exists == false
                           ? null
                           : (from line in Environment.DescriptorFile.ReadLines()
                              let version = line.GenerateVersionNumber().ToVersion()
                              where version != null
                              select version).FirstOrDefault();
        }

        IPackageBuilder CreateBuilder(string commandLine)
        {
            IPackageBuilder builder;
            switch (commandLine)
            {
                case "$meta":
                    builder = new MetaPackageBuilder(Environment);
                    break;
                default:
                    builder = new ConventionMSBuildEngine(Environment);
                    break;
            }
            return builder;
        }

        string GetCurrentVersion()
        {
            var version = ReadVersionFile()
                          ?? (Environment.Descriptor.Version != null ? Environment.Descriptor.Version.ToString() : null);

            if (version == null)
                throw new InvalidOperationException("No package version found either in the descriptor or version file.");
            return version;
        }

        ICommandOutput NoDescriptorFound()
        {
            return Environment.Descriptor == null
                           ? new GenericError("Could not find a wrap descriptor. Are you in a project directory?")
                           : null;
        }

        string ReadVersionFile()
        {
            var versionFile = Environment.CurrentDirectory.GetFile("version");
            if (versionFile.Exists)
                using (var stream = versionFile.OpenRead())
                using (var streamReader = new StreamReader(stream, Encoding.UTF8))
                    return streamReader.ReadLine();
            return null;
        }

        ICommandOutput VerifyPath()
        {
            if (Path != null)
            {
                _destinationPath = FileSystem.GetDirectory(Path);
                if (_destinationPath.Exists == false)
                    return new GenericError("Path '{0}' doesn't exist.", Path);
            }
            return null;
        }
    }
}