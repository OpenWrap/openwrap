using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using OpenFileSystem.IO;
using OpenWrap.Build;
using OpenWrap.Build.BuildEngines;
using OpenWrap.Repositories;
using OpenWrap.Services;

namespace OpenWrap.Commands.Wrap
{
    [Command(Noun = "Wrap", Verb = "Build")]
    public class BuildWrapCommand : AbstractCommand
    {
        IDirectory _destinationPath;

        [CommandInput]
        public string Name { get; set; }

        [CommandInput]
        public string Path { get; set; }

        protected IEnvironment Environment
        {
            get { return WrapServices.GetService<IEnvironment>(); }
        }

        protected IFileSystem FileSystem
        {
            get { return WrapServices.GetService<IFileSystem>(); }
        }

        public override IEnumerable<ICommandOutput> Execute()
        {
            return Either(NoDescriptorFound)
                .Or(VerifyPath)
                .Or(Build());
        }

        IEnumerable<ICommandOutput> Build()
        {
            var packageName = Name ?? Environment.Descriptor.Name;
            var buildFiles = new List<FileBuildResult>();
            var commandLine = Environment.Descriptor.BuildCommand;
            var destinationPath = _destinationPath ?? Environment.CurrentDirectory;
            if (commandLine == null)
                foreach (var t in new ConventionMSBuildEngine(Environment).Build())
                {
                    if (t is TextBuildResult)
                        yield return new GenericMessage(((TextBuildResult)t).Text);
                    else if (t is FileBuildResult)
                    {
                        var buildResult = (FileBuildResult)t;
                        buildFiles.Add(buildResult);
                        yield return new GenericMessage(string.Format("Output found - {0}: '{1}'", buildResult.ExportName, buildResult.Path));
                    }
                }
            if (buildFiles.Count > 0)
            {
                var version = GetCurrentVersion().GenerateVersionNumber();
                var packageFilePath = destinationPath.GetFile(packageName + "-" + version + ".wrap");
                PackagePackager.CreatePackage(
                        packageFilePath,
                        version,
                        buildFiles.GroupBy(x => x.ExportName, x => FileSystem.GetFile(x.Path.FullPath)));
                yield return new GenericMessage(string.Format("Package built at '{0}'.", packageFilePath));
            }
        }

        string GetCurrentVersion()
        {
            var version = ReadVersionFile() 
                ?? (Environment.Descriptor.Version != null ? Environment.Descriptor.Version.ToString() : null);
            
            if (version == null)
                throw new InvalidOperationException("No package version found either on the command line, in descriptor or version file");
            return version;
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
        ICommandOutput NoDescriptorFound()
        {
            return Environment.Descriptor == null 
                ? new GenericError("Could not find a wrap descriptor. Are you in a project directory?")
                : null;
        }
    }
}