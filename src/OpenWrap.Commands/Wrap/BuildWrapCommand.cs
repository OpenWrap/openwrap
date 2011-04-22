using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using OpenFileSystem.IO;
using OpenWrap.Build;
using OpenWrap.Build.PackageBuilders;
using OpenWrap.Collections;
using OpenWrap.IO;
using OpenWrap.IO.Packaging;
using OpenWrap.PackageModel;
using OpenWrap.PackageModel.Serialization;
using OpenWrap.Repositories;
using OpenWrap.Runtime;
using OpenWrap.Services;


namespace OpenWrap.Commands.Wrap
{
    [Command(Noun = "wrap", Verb = "build", Description = "Builds all projects and creates a wrap package.")]
    public class BuildWrapCommand : AbstractCommand
    {
        IDirectory _destinationPath;
        IList<FileBuildResult> _buildResults = new List<FileBuildResult>();
        IEnumerable<IPackageBuilder> _builders;

        [CommandInput]
        public string Name { get; set; }

        [CommandInput]
        public string Path { get; set; }

        [CommandInput]
        public bool Quiet { get; set; }

        [CommandInput]
        public string From { get; set; }

        [CommandInput]
        public IEnumerable<string> Flavour { get; set; }

        IEnvironment _environment;

        readonly IFileSystem _fileSystem;
        IDirectory _currentDirectory;

        public BuildWrapCommand()
            : this(ServiceLocator.GetService<IFileSystem>(), ServiceLocator.GetService<IEnvironment>())
        {

        }
        public BuildWrapCommand(IFileSystem fileSystem, IEnvironment environment)
        {
            _fileSystem = fileSystem;
            _environment = environment;
            _currentDirectory = environment.CurrentDirectory;
        }
        protected override IEnumerable<Func<IEnumerable<ICommandOutput>>> Validators()
        {
            yield return FromEnvironmentNotFound;
            yield return NoDescriptorFound;
            yield return VerifyPath;
            yield return CreateBuilder;
        }
        protected override IEnumerable<ICommandOutput> ExecuteCore()
        {
            return CreateBuilder().Concat(TriggerBuild()).Concat(Build());
        }

        IEnumerable<ICommandOutput> Build()
        {

            var packageName = Name ?? _environment.Descriptor.Name;
            var destinationPath = _destinationPath ?? _currentDirectory;

            var packageDescriptorForEmbedding = new PackageDescriptor(GetCurrentPackageDescriptor());

            var generatedVersion = GetPackageVersion(_buildResults, packageDescriptorForEmbedding);

            if (generatedVersion == null)
                yield return new Error("Could not build package, no version found.");

            packageDescriptorForEmbedding.Version = generatedVersion;
            packageDescriptorForEmbedding.Name = packageName;

            var packageFilePath = destinationPath.GetFile(
                PackageNameUtility.PackageFileName(packageName, generatedVersion.ToString()));

            var packageContent = GeneratePackageContent(_buildResults).Concat(
                                    GenerateVersionFile(generatedVersion),
                                    GenerateDescriptorFile(packageDescriptorForEmbedding)
                                 ).ToList();
            foreach (var item in packageContent)
                yield return new GenericMessage(string.Format("Copying: {0}/{1}{2}", item.RelativePath, item.FileName, FormatBytes(item.Size)));

            Packager.NewFromFiles(packageFilePath, packageContent);
            yield return new GenericMessage(string.Format("Package built at '{0}'.", packageFilePath));

        }

        string FormatBytes(long? size)
        {
            if (size == null) return string.Empty;
            return string.Format(" ({0} bytes)", ((long)size).ToString("N0"));
        }
        IEnumerable<ICommandOutput> FromEnvironmentNotFound()
        {
            if (From != null)
            {
                var directory = _fileSystem.GetDirectory(From);
                if (directory.Exists == false)
                {
                    yield return new Error("Directory '{0}' not found.", From);
                    yield break;
                }
                var newEnv = new CurrentDirectoryEnvironment(directory);
                newEnv.Initialize();
                if (newEnv.ScopedDescriptors.Any() == false)
                {
                    yield return new Error("No descriptor found in directory '{0}'.", From);
                    yield break;
                }

                _environment = newEnv;
                //return new Info("Building package at '{0}'.", From);
            }
        }

        IEnumerable<ICommandOutput> TriggerBuild()
        {
            _buildResults.Clear();
            foreach (var m in ProcessBuildResults(_builders, _buildResults.Add)) yield return m;
            _buildResults = _buildResults.Distinct().ToList();
        }

        IPackageDescriptor GetCurrentPackageDescriptor()
        {
            foreach (var file in _buildResults.Where(x => x.ExportName == "." && x.FileName.EndsWithNoCase(".wrapdesc")).ToList())
                _buildResults.Remove(file);

            return _environment.Descriptor;
        }

        IEnumerable<PackageContent> GeneratePackageContent(IEnumerable<FileBuildResult> buildFiles)
        {
            var binFiles = (from fileDescriptor in buildFiles
                            where fileDescriptor.ExportName.StartsWith("bin-")
                            from file in ResolveFiles(fileDescriptor)
                            where file.Exists
                            select new PackageContent
                            {
                                FileName = file.Name,
                                RelativePath = fileDescriptor.ExportName,
                                Size = file.Size,
                                Stream = () => file.OpenRead()
                            }).ToList();

            var externalFiles = from fileDesc in buildFiles
                                where fileDesc.ExportName.StartsWith("bin-") == false
                                from file in ResolveFiles(fileDesc)
                                where file.Exists &&
                                      (fileDesc.AllowBinDuplicate ||
                                       binFiles.Any(x => x.FileName == file.Name) == false)
                                select new PackageContent
                                {
                                    FileName = file.Name,
                                    Size = file.Size,
                                    RelativePath = fileDesc.ExportName,
                                    Stream = () => file.OpenRead()
                                };
            return binFiles.Concat(externalFiles);
        }

        IEnumerable<IFile> ResolveFiles(FileBuildResult fileDescriptor)
        {
            return fileDescriptor.Path.FullPath.Contains("*")
                           ? _fileSystem.Files(fileDescriptor.Path.FullPath)
                           : new[] { _fileSystem.GetFile(fileDescriptor.Path.FullPath) };
        }

        IEnumerable<ICommandOutput> ProcessBuildResults(IEnumerable<IPackageBuilder> packageBuilders, Action<FileBuildResult> onFound)
        {
            foreach (var t in packageBuilders.SelectMany(x => x.Build()))
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

        static PackageContent GenerateDescriptorFile(PackageDescriptor descriptor)
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

        static PackageContent GenerateVersionFile(Version generatedVersion)
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

        static bool IsVersion(FileBuildResult build)
        {
            return build.ExportName == "." && build.FileName.EqualsNoCase("version");
        }
        Version GetVersionFromVersionFiles(IList<FileBuildResult> buildFiles)
        {

            var generatedVersion = (from buildContent in buildFiles
                                    where IsVersion(buildContent)
                                    let file = _fileSystem.GetFile(buildContent.Path.FullPath)
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
            var versionFile = _environment.DescriptorFile != null && _environment.DescriptorFile.Exists
                                  ? _environment.DescriptorFile.Parent.GetFile("version")
                                  : null;
            return versionFile == null || versionFile.Exists == false
                           ? null
                           : (from line in versionFile.ReadLines()
                              let version = line.GenerateVersionNumber().ToVersion()
                              where version != null
                              select version).FirstOrDefault();
        }

        IEnumerable<ICommandOutput> CreateBuilder()
        {
            _builders = (
                                from commandLine in _environment.Descriptor.Build.DefaultIfEmpty("msbuild")
                                let builder = ChooseBuilderInstance(commandLine)
                                let parameters = from segment in commandLine.Split(';').Skip(1)
                                                 let keyValues = segment.Split('=')
                                                 where keyValues.Length >= 2
                                                 let key = keyValues[0]
                                                 let value = segment.Substring(key.Length + 1).Trim()
                                                 group value by key.Trim()
                                select AssignProperties(builder, parameters)
                        ).ToList();
            yield break;
        }

        IPackageBuilder AssignProperties(IPackageBuilder builder, IEnumerable<IGrouping<string, string>> properties)
        {
            foreach (var property in properties)
            {
                var pi = builder.GetType().GetProperty(property.Key, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
                if (pi != null)
                    pi.SetValue(builder, property.ToList(), null);
            }
            return builder;
        }

        IPackageBuilder ChooseBuilderInstance(string commandLine)
        {
            commandLine = commandLine.Trim();
            if (commandLine.StartsWithNoCase("msbuild"))
                return new MSBuildPackageBuilder(_fileSystem, _environment, new DefaultFileBuildResultParser());
            if (commandLine.StartsWithNoCase("files"))
                return new FilePackageBuilder();
            if (commandLine.StartsWithNoCase("command"))
                return new CommandLinePackageBuilder(_fileSystem, _environment, new DefaultFileBuildResultParser());
            return new NullPackageBuilder(_environment);
        }

        string GetCurrentVersion()
        {
            var version = ReadVersionFile()
                          ?? (_environment.Descriptor.Version != null ? _environment.Descriptor.Version.ToString() : null);

            if (version == null)
                throw new InvalidOperationException("No package version found either in the descriptor or version file.");
            return version;
        }

        IEnumerable<ICommandOutput> NoDescriptorFound()
        {
            if (_environment.ScopedDescriptors.Any() == false)
                yield return new Error("Could not find any descriptor. Are you in a project directory?");
        }

        string ReadVersionFile()
        {
            var versionFile = _environment.CurrentDirectory.GetFile("version");
            if (versionFile.Exists)
                using (var stream = versionFile.OpenRead())
                using (var streamReader = new StreamReader(stream, Encoding.UTF8))
                    return streamReader.ReadLine();
            return null;
        }

        IEnumerable<ICommandOutput> VerifyPath()
        {
            if (Path != null)
            {
                _destinationPath = _fileSystem.GetDirectory(Path);
                if (_destinationPath.Exists == false)
                    yield return new Error("Path '{0}' doesn't exist.", Path);
            }
        }
    }
}