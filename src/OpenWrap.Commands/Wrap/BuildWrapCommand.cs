using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using OpenFileSystem.IO;
using OpenWrap.Build;
using OpenWrap.Build.PackageBuilders;
using OpenWrap.Collections;
using OpenWrap.Commands.Messages;
using OpenWrap.IO;
using OpenWrap.IO.Packaging;
using OpenWrap.PackageModel;
using OpenWrap.PackageModel.Serialization;
using OpenWrap.Runtime;
using OpenWrap.Services;


namespace OpenWrap.Commands.Wrap
{
    [Command(Noun = "wrap", Verb = "build")]
    public class BuildWrapCommand : AbstractCommand, ICommandWithWildcards
    {
        readonly IDirectory _currentDirectory;
        readonly IFileSystem _fileSystem;
        IList<FileBuildResult> _buildResults = new List<FileBuildResult>();
        IEnumerable<IPackageBuilder> _builders;
        IDirectory _destinationPath;
        IEnvironment _environment;
        SemanticVersion _generatedVersion;
        IFile _sharedAssemblyInfoFile;
        ILookup<string, string> _wildcards;

        public BuildWrapCommand()
            : this(ServiceLocator.GetService<IFileSystem>(), ServiceLocator.GetService<IEnvironment>())
        {
        }

        public BuildWrapCommand(IFileSystem fileSystem, IEnvironment environment)
        {
            _fileSystem = fileSystem;
            _environment = environment;
            _currentDirectory = environment.CurrentDirectory;
            _wildcards = new string[0].ToLookup(_ => _, _ => default(string));
        }

        [CommandInput]
        public string Configuration { get; set; }

        [CommandInput]
        public bool Debug
        {
            get { return Configuration.EqualsNoCase("Debug"); }
            set { Configuration = "Debug"; }
        }

        [CommandInput]
        public IEnumerable<string> Flavour { get; set; }

        [CommandInput]
        public string From { get; set; }

        [CommandInput]
        public bool Incremental { get; set; }

        [CommandInput]
        public string Name { get; set; }

        [CommandInput]
        public string Path { get; set; }

        [CommandInput]
        public bool Quiet { get; set; }

        [CommandInput]
        public bool Release
        {
            get { return Configuration.EqualsNoCase("Release"); }
            set { Configuration = "Release"; }
        }

        [CommandInput]
        public string Version { get; set; }

        public void Wildcards(ILookup<string, string> values)
        {
            _wildcards = values;
        }

        protected override IEnumerable<ICommandOutput> ExecuteCore()
        {
            using (CreateSharedAssemblyInfo())
            {
                foreach (var m in CreateBuilder()) yield return m;
                foreach (var m in Build())
                {
                    yield return m;
                    if (m.Type == CommandResultType.Error)
                        yield break;
                }
                foreach (var m in Package()) yield return m;
            }
        }

        protected override IEnumerable<Func<IEnumerable<ICommandOutput>>> Validators()
        {
            yield return SetEnvironmentToFromInput;
            yield return VerifyDescriptorPresent;
            yield return VerifyVersion;
            yield return SetOutputPath;
            yield return CreateBuilder;
        }

        static IPackageBuilder AssignProperties(IPackageBuilder builder, IEnumerable<IGrouping<string, string>> properties)
        {
            var unknown = new List<KeyValuePair<string, string>>();


            var builderType = builder.GetType();
            foreach (var property in properties)
            {
                var pi = builderType.GetProperty(property.Key, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
                if (pi != null)
                {
                    bool boolProperty;
                    if (typeof(IEnumerable<string>).IsAssignableFrom(pi.PropertyType))
                    {
                        var existingValues = pi.GetValue(builder, null) as IEnumerable<string>;
                        if (existingValues == null || existingValues.Count() == 0)
                            pi.SetValue(builder, property.ToList(), null);
                    }
                    else if (pi.PropertyType == typeof(string) && property.Count() > 0)
                    {
                        pi.SetValue(builder, property.Last(), null);
                    }
                    else if (pi.PropertyType == typeof(bool) && property.Count() > 0 && bool.TryParse(property.Last(), out boolProperty))
                    {
                        pi.SetValue(builder, boolProperty, null);
                    }
                }
                else
                {
                    unknown.AddRange(property.Select(x => new KeyValuePair<string, string>(property.Key, x)));
                }
            }
            var propertiesPi = builderType.GetProperty("Properties", BindingFlags.Instance | BindingFlags.Public);
            if (propertiesPi != null && typeof(IEnumerable<IGrouping<string, string>>).IsAssignableFrom(propertiesPi.PropertyType))
            {
                var unknownLookup = unknown.ToLookup(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
                propertiesPi.SetValue(builder, unknownLookup, null);
            }
            return builder;
        }

        static PackageContent GenerateDescriptorFile(PackageDescriptor descriptor)
        {
            var descriptorStream = new MemoryStream();
            new PackageDescriptorReaderWriter().Write(descriptor.GetPersistableEntries(), descriptorStream);
            return new PackageContent
            {
                FileName = descriptor.Name + ".wrapdesc",
                RelativePath = ".",
                Stream = descriptorStream.ResetOnRead(),
                Size = descriptorStream.Length
            };
        }

        static PackageContent GenerateVersionFile(SemanticVersion generatedVersion)
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

        IEnumerable<ICommandOutput> Build()
        {
            _buildResults.Clear();
            var buildTime = new Stopwatch();
            buildTime.Start();
            foreach (var m in ProcessBuildResults(_builders, _buildResults.Add)) yield return m;
            yield return new Info("Build completed in {0}.", buildTime.Elapsed);
            _buildResults = _buildResults.Distinct().ToList();
        }


        IPackageBuilder ChooseBuilderInstance(string commandLine)
        {
            commandLine = commandLine.Trim();
            var isMsBuild = commandLine.StartsWithNoCase("msbuild");
            var isDefault = commandLine.StartsWithNoCase("default");
            var isxbuild = commandLine.StartsWithNoCase("xbuild");
            if (isMsBuild || isDefault || isxbuild)
            {
                var configValue = isDefault ? "default" : (isMsBuild ? "msbuild" : "xbuild");
                var builder = new MSBuildPackageBuilder(_fileSystem, _environment, new DefaultFileBuildResultParser())
                {
                    Incremental = Incremental,
                    BuildEngine = configValue
                };
                return builder;
            }
            if (commandLine.StartsWithNoCase("files"))
                return new FilePackageBuilder();
            if (commandLine.StartsWithNoCase("command") || commandLine.StartsWithNoCase("process"))
                return new CommandLinePackageBuilder(_fileSystem, _environment, new DefaultFileBuildResultParser());
            if (commandLine.StartsWithNoCase("custom"))
                return CreateCustomBuilder(commandLine);
            return new NullPackageBuilder(_environment);
        }

        IEnumerable<ICommandOutput> CreateBuilder()
        {
            _builders = (
                            from commandLine in _environment.Descriptor.Build.DefaultIfEmpty("default")
                            let builder = ChooseBuilderInstance(commandLine)
                            let parameters = ParseBuilderProperties(commandLine)
                            select AssignProperties(builder, OverrideWithInputs(parameters))
                        ).ToList();
            yield break;
        }

        IPackageBuilder CreateCustomBuilder(string commandLine)
        {
            var typeName = ParseBuilderProperties(commandLine).Where(x => x.Key.EqualsNoCase("TypeName")).Select(x => x.FirstOrDefault()).FirstOrDefault();
            if (typeName == null)
                return new ErrorPackageBuilder("Cannot find a TypeName parameter to the custom build provider.");

            var type = Type.GetType(typeName, false);
            if (type == null)
                return new ErrorPackageBuilder(string.Format("Cannot load type '{0}'. Make sure the type exists and is avaialble.", typeName));

            return (IPackageBuilder)Activator.CreateInstance(type);
        }

        IDisposable CreateSharedAssemblyInfo()
        {
            if (_generatedVersion != null && _environment.Descriptor.AssemblyInfo.Any())
            {
                _sharedAssemblyInfoFile = BuildManagement.TryGenerateAssemblyInfo(_environment.DescriptorFile, _generatedVersion);
                return new ActionOnDispose(_sharedAssemblyInfoFile.Delete);
            }
            return new ActionOnDispose(() => { });
        }

        string FormatBytes(long? size)
        {
            if (size == null) return string.Empty;
            return string.Format(" ({0} bytes)", ((long)size).ToString("N0"));
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

        IPackageDescriptor GetCurrentPackageDescriptor()
        {
            foreach (var file in _buildResults.Where(x => x.ExportName == "." && x.FileName.EndsWithNoCase(".wrapdesc")).ToList())
                _buildResults.Remove(file);

            return _environment.Descriptor;
        }

        IEnumerable<IGrouping<string, string>> OverrideWithInputs(IEnumerable<IGrouping<string, string>> parameters)
        {
            var overrides = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (Configuration != null)
                overrides.Add("Configuration", Configuration);
            if (Incremental)
                overrides.Add("Incremental", "True");
            overrides.Add("OpenWrap-CurrentBuildVersion", _generatedVersion.ToString());
            if (_sharedAssemblyInfoFile != null)
                overrides.Add("OpenWrap-SharedAssemblyInfoFile", _sharedAssemblyInfoFile.Path.FullPath);

            return overrides.GroupBy(x => x.Key, x => x.Value).Concat(_wildcards.Concat(parameters).Where(param => !overrides.ContainsKey(param.Key)));
        }

        IEnumerable<ICommandOutput> Package()
        {
            var packageName = Name ?? _environment.Descriptor.Name;

            var packageDescriptorForEmbedding = new PackageDescriptor(GetCurrentPackageDescriptor());

            packageDescriptorForEmbedding.SemanticVersion = _generatedVersion;
            packageDescriptorForEmbedding.Name = packageName;

            var packageFilePath = _destinationPath.GetFile(
                PackageNameUtility.PackageFileName(packageName, _generatedVersion.ToString()));

            var packageContent = GeneratePackageContent(_buildResults)
                .Concat(
                    GenerateVersionFile(_generatedVersion),
                    GenerateDescriptorFile(packageDescriptorForEmbedding)
                ).ToList();
            foreach (var item in packageContent)
                yield return new Info(string.Format("Copying: {0}/{1}{2}", item.RelativePath, item.FileName, FormatBytes(item.Size)));

            Packager.NewFromFiles(packageFilePath, packageContent);
            yield return new PackageBuilt(packageFilePath);
        }

        IEnumerable<IGrouping<string, string>> ParseBuilderProperties(string commandLine)
        {
            return from segment in commandLine.Split(';').Skip(1)
                   let keyValues = segment.Split('=')
                   where keyValues.Length >= 2
                   let key = keyValues[0]
                   let value = segment.Substring(key.Length + 1).Trim()
                   group value by key.Trim();
        }


        IEnumerable<ICommandOutput> ProcessBuildResults(IEnumerable<IPackageBuilder> packageBuilders, Action<FileBuildResult> onFound)
        {
            foreach (var t in packageBuilders.SelectMany(x => x.Build()))
            {
                if (t is InfoBuildResult || (t is TextBuildResult && !Quiet))
                    yield return new Info(t.Message);
                else if (t is FileBuildResult)
                {
                    var buildResult = (FileBuildResult)t;

                    onFound(buildResult);
                    if (!Quiet)
                        yield return new Info(string.Format("Output found - {0}: '{1}'", buildResult.ExportName, buildResult.Path));
                }
                else if (t is ErrorBuildResult)
                {
                    yield return new Error(t.Message);
                    yield break;
                }
            }
        }

        IEnumerable<IFile> ResolveFiles(FileBuildResult fileDescriptor)
        {
            return fileDescriptor.Path.FullPath.Contains("*")
                       ? _fileSystem.Files(fileDescriptor.Path.FullPath)
                       : new[] { _fileSystem.GetFile(fileDescriptor.Path.FullPath) };
        }

        IEnumerable<ICommandOutput> SetEnvironmentToFromInput()
        {
            if (From != null)
            {
                var directory = _fileSystem.GetDirectory(From);
                if (directory.Exists == false)
                {
                    yield return new DirectoryNotFound(directory);
                    yield break;
                }
                var newEnv = new CurrentDirectoryEnvironment(directory);
                newEnv.Initialize();


                _environment = newEnv;
            }
        }

        IEnumerable<ICommandOutput> SetOutputPath()
        {
            _destinationPath = Path != null
                                   ? _fileSystem.GetDirectory(Path).MustExist()
                                   : _fileSystem.GetCurrentDirectory();
            yield break;
        }

        IEnumerable<ICommandOutput> VerifyDescriptorPresent()
        {
            if (_environment.ScopedDescriptors.Any() == false)
            {
                yield return new PackageDescriptorNotFound(_environment.CurrentDirectory);
                yield break;
            }
        }

        IEnumerable<ICommandOutput> VerifyVersion()
        {
            var versionFile = _environment.CurrentDirectory.GetFile("version");
            if (Version == null && !versionFile.Exists && _environment.Descriptor.SemanticVersion == null)
            {
                yield return new PackageVersionMissing();
                yield break;
            }
            if (Version == null)
            {
                _generatedVersion = BuildManagement.GenerateVersion(
                    _environment.Descriptor,
                    versionFile);
            }
            else
                _generatedVersion = Version.ToSemVer();
        }
    }
}