using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using System.Reflection;
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
        IPackageBuilder _builder;
        IList<FileBuildResult> _buildResults = new List<FileBuildResult>();

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
                    .Or(CreateBuilder)
                    .Or(
                        TriggerBuild()
                        .Concat(Build())
                    );
        }

        IEnumerable<ICommandOutput> Build()
        {
            var packageName = Name ?? Environment.Descriptor.Name;
            var destinationPath = _destinationPath ?? Environment.CurrentDirectory;

            var packageDescriptorForEmbedding = new PackageDescriptor(GetCurrentPackageDescriptor());

            var generatedVersion = GetPackageVersion(_buildResults, packageDescriptorForEmbedding);

            if (generatedVersion == null)
                yield return new Error("Could not build package, no version found.");

            packageDescriptorForEmbedding.Version = generatedVersion;
            packageDescriptorForEmbedding.Name = packageName;

            foreach (var file in _buildResults)
                yield return new GenericMessage(string.Format("Copying: {0} - {1}", file.ExportName, file.Path));

            var packageFilePath = destinationPath.GetFile(
                PackageNameUtility.PacakgeFileName(packageName, generatedVersion.ToString()));

            var packageContent = GeneratePackageContent(_buildResults).Concat(
                                    GenerateVersionFile(generatedVersion),
                                    GenerateDescriptorFile(packageDescriptorForEmbedding)
                                 );

            PackageBuilder.NewFromFiles(packageFilePath, packageContent);
            yield return new GenericMessage(string.Format("Package built at '{0}'.", packageFilePath));

        }

        IEnumerable<ICommandOutput> TriggerBuild()
        {
            _buildResults.Clear();
            foreach (var m in ProcessBuildResults(_builder, _buildResults.Add)) yield return m;
            _buildResults = _buildResults.Distinct().ToList();
        }

        PackageDescriptor GetCurrentPackageDescriptor()
        {
            foreach (var file in _buildResults.Where(x => x.ExportName == "." && x.FileName.EndsWithNoCase(".wrapdesc")).ToList())
                _buildResults.Remove(file);

            return Environment.Descriptor;
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
                    if (!Quiet)
                        yield return new GenericMessage(string.Format("Output found - {0}: '{1}'", buildResult.ExportName, buildResult.Path));
                }
                else if (t is ErrorBuildResult)
                {
                    yield return new Error(t.Message);
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

        Version GetPackageVersion(IList<FileBuildResult> buildFiles, PackageDescriptor packageDescriptorForEmbedding)
        {
            // gets the package version from (in this order):
            // 1. 'version' file generated by the build
            // 2. 'version' file living alongside the .wrapdesc file
            // 3. 'version:' header in wrap descriptor

            return new DefaultPackageInfo(string.Empty, GetVersionFromVersionFiles(buildFiles), packageDescriptorForEmbedding).Version;
        }

        bool IsVersion(FileBuildResult build)
        {
            return build.ExportName == "." && build.FileName.EqualsNoCase("version");
        }
        Version GetVersionFromVersionFiles(IList<FileBuildResult> buildFiles)
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
            var versionFile = Environment.DescriptorFile != null && Environment.DescriptorFile.Exists
                                  ? Environment.DescriptorFile.Parent.GetFile("version")
                                  : null;
            return versionFile == null || versionFile.Exists == false
                           ? null
                           : (from line in versionFile.ReadLines()
                              let version = line.GenerateVersionNumber().ToVersion()
                              where version != null
                              select version).FirstOrDefault();
        }

        ICommandOutput CreateBuilder()
        {
            var commandLine = Environment.Descriptor.BuildCommand;
            if (string.IsNullOrEmpty(commandLine))
                commandLine = "msbuild";
            var properties = from segment in commandLine.Split(';').Skip(1)
                             let keyValues = segment.Split('=')
                             where keyValues.Length >= 2
                             let key = keyValues[0]
                             let value = segment.Substring(key.Length + 1)
                             group new { key, value } by key;

            _builder = commandLine.Trim().StartsWith("msbuild", StringComparison.OrdinalIgnoreCase)
                                  ? (IPackageBuilder)new MSBuildBuildEngine(FileSystem, Environment)
                                  : new MetaPackageBuilder(Environment);
            foreach(var property in properties)
            {
                var pi = _builder.GetType().GetProperty(property.Key, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
                if (pi != null)
                    pi.SetValue(_builder, property.Select(x=>x.value).ToList(), null);
            }
            return null;
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
                           ? new Error("Could not find a wrap descriptor. Are you in a project directory?")
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
                    return new Error("Path '{0}' doesn't exist.", Path);
            }
            return null;
        }
    }
}