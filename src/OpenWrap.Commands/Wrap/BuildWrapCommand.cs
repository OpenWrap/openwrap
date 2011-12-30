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
    [Command(Noun = "wrap", Verb = "build", Description = "Builds all projects and creates a wrap package.")]
    public class BuildWrapCommand : AbstractCommand
    {
        readonly IDirectory _currentDirectory;
        readonly IFileSystem _fileSystem;
        IList<FileBuildResult> _buildResults = new List<FileBuildResult>();
        IEnumerable<IPackageBuilder> _builders;
        IDirectory _destinationPath;
        IEnvironment _environment;

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
        public IEnumerable<string> Properties { get; set; }

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
        public string Version { get; set; }

        [CommandInput]
        public string Targets { get; set; }

        [CommandInput]
        public bool Release
        {
            get { return Configuration.EqualsNoCase("Release"); }
            set { Configuration = "Release"; }
        }

        protected override IEnumerable<ICommandOutput> ExecuteCore()
        {
            foreach (var m in CreateBuilder()) yield return m;
            foreach(var m in TriggerBuild())
            {
                yield return m;
                if (m.Type == CommandResultType.Error)
                    yield break;
            }
            foreach (var m in Build()) yield return m;
        }

        protected override IEnumerable<Func<IEnumerable<ICommandOutput>>> Validators()
        {
            yield return SetEnvironmentToFromInput;
            yield return VerifyDescriptorPresent;
            yield return VerifyVersion;
            yield return SetOutputPath;
            yield return CreateBuilder;
        }

        IEnumerable<ICommandOutput> SetOutputPath()
        {
            _destinationPath = Path != null
                ? _fileSystem.GetDirectory(Path).MustExist()
                : _fileSystem.GetCurrentDirectory();
            yield break;
        }

        IEnumerable<ICommandOutput> VerifyVersion()
        {
            if (_environment.Descriptor.Version == null && _environment.CurrentDirectory.GetFile("version").Exists == false)
                yield return new PackageVersionMissing();
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

        static bool IsVersion(FileBuildResult build)
        {
            return build.ExportName == "." && build.FileName.EqualsNoCase("version");
        }

        static IPackageBuilder AssignProperties(IPackageBuilder builder, IEnumerable<IGrouping<string, string>> properties)
        {
            List<KeyValuePair<string, string>> unknown = new List<KeyValuePair<string, string>>();


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
                        
                        unknown.AddRange(property.Select(x=>new KeyValuePair<string,string>(property.Key, x)));
                    }
            }
            var propertiesPi = builderType.GetProperty("Properties", BindingFlags.Instance | BindingFlags.Public);
            if (propertiesPi != null && typeof(IEnumerable<IGrouping<string,string>>).IsAssignableFrom(propertiesPi.PropertyType))
            {
                var unknownLookup = unknown.ToLookup(x=>x.Key, x=>x.Value);
                propertiesPi.SetValue(builder, unknownLookup, null);
            }
            return builder;
        }

        IEnumerable<ICommandOutput> Build()
        {
            
            var packageName = Name ?? _environment.Descriptor.Name;

            var packageDescriptorForEmbedding = new PackageDescriptor(GetCurrentPackageDescriptor());

            var generatedVersion = GetPackageVersion(_buildResults, packageDescriptorForEmbedding);

            if (generatedVersion == null)
            {
                yield return new Error("Could not build package, no version found.");
                yield break;
            }
            packageDescriptorForEmbedding.Version = generatedVersion;
            packageDescriptorForEmbedding.Name = packageName;

            var packageFilePath = _destinationPath.GetFile(
                PackageNameUtility.PackageFileName(packageName, generatedVersion.ToString()));

            var packageContent = GeneratePackageContent(_buildResults)
                .Concat(
                    GenerateVersionFile(generatedVersion),
                    GenerateDescriptorFile(packageDescriptorForEmbedding)
                ).ToList();
            foreach (var item in packageContent)
                yield return new Info(string.Format("Copying: {0}/{1}{2}", item.RelativePath, item.FileName, FormatBytes(item.Size)));

            Packager.NewFromFiles(packageFilePath, packageContent);
            yield return new PackageBuilt(packageFilePath);
        }

        IPackageBuilder ChooseBuilderInstance(string commandLine)
        {
            commandLine = commandLine.Trim();
            if (commandLine.StartsWithNoCase("msbuild"))
            {
                var builder = new MSBuildPackageBuilder(_fileSystem, _environment, new DefaultFileBuildResultParser())
                {
                    Incremental = Incremental,
                    Targets = Targets
                };
                if (Properties != null && Properties.Count() > 0)
                {
                    builder.Properties = Properties.ToLookup(p => p.Split('=')[0], p => p.Split('=')[1]);
                }
                return builder;
            }
            if (commandLine.StartsWithNoCase("files"))
                return new FilePackageBuilder();
            if (commandLine.StartsWithNoCase("command"))
                return new CommandLinePackageBuilder(_fileSystem, _environment, new DefaultFileBuildResultParser());
            if (commandLine.StartsWithNoCase("custom"))
                return CreateCustomBuilder(commandLine);
            return new NullPackageBuilder(_environment);
        }

        IPackageBuilder CreateCustomBuilder(string commandLine)
        {
            var typeName = ParseBuilderProperties(commandLine).Where(x => x.Key.EqualsNoCase("TypeName")).Select(x=>x.FirstOrDefault()).FirstOrDefault();
            if (typeName == null)
                return new ErrorPackageBuilder("Cannot find a TypeName parameter to the custom build provider.");

            var type = Type.GetType(typeName, false);
            if (type == null)
                return new ErrorPackageBuilder(string.Format("Cannot load type '{0}'. Make sure the type exists and is avaialble.", typeName));

            return (IPackageBuilder)Activator.CreateInstance(type);
        }

        IEnumerable<ICommandOutput> CreateBuilder()
        {
            _builders = (
                            from commandLine in _environment.Descriptor.Build.DefaultIfEmpty("msbuild")
                            let builder = ChooseBuilderInstance(commandLine)
                            let parameters = ParseBuilderProperties(commandLine)
                            select AssignProperties(builder, OverrideWithInputs(parameters))
                        ).ToList();
            yield break;
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

        IEnumerable<IGrouping<string, string>> OverrideWithInputs(IEnumerable<IGrouping<string,string>> parameters)
        {
            var overrides = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (Configuration != null)
                overrides.Add("Configuration", Configuration);
            if (Incremental)
                overrides.Add("Incremental", "True");

            return overrides.GroupBy(x => x.Key, x => x.Value).Concat(parameters.Where(param => !overrides.ContainsKey(param.Key)));

        }

        string FormatBytes(long? size)
        {
            if (size == null) return string.Empty;
            return string.Format(" ({0} bytes)", ((long)size).ToString("N0"));
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
                //return new Info("Building package at '{0}'.", From);
            }
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

        Version GetPackageVersion(IList<FileBuildResult> buildFiles, PackageDescriptor packageDescriptorForEmbedding)
        {
            // gets the package version from (in this order):
            // 0. -version input
            // 1. 'version' file generated by the build
            // 2. 'version' file living alongside the .wrapdesc file
            // 3. 'version:' header in wrap descriptor

            return Version != null
                ? Version.GenerateVersionNumber().ToVersion() 
                : new DefaultPackageInfo(string.Empty, GetVersionFromVersionFiles(buildFiles), packageDescriptorForEmbedding).Version;
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

        IEnumerable<ICommandOutput> VerifyDescriptorPresent()
        {
            if (_environment.ScopedDescriptors.Any() == false)
            {
                yield return new PackageDescriptorNotFound(_environment.CurrentDirectory);
                yield break;
            }
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

        IEnumerable<ICommandOutput> TriggerBuild()
        {
            _buildResults.Clear();
            var buildTime = new Stopwatch();
            buildTime.Start();
            foreach (var m in ProcessBuildResults(_builders, _buildResults.Add)) yield return m;
            yield return new Info("Build completed in {0}.", buildTime.Elapsed);
            _buildResults = _buildResults.Distinct().ToList();
        }

    }
}