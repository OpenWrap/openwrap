using System;
using System.Collections.Generic;
<<<<<<< HEAD
using System.Diagnostics;
using System.Linq;
using System.Text;
using OpenFileSystem.IO;
=======
using System.IO;
using System.Linq;
using System.Text;
using OpenFileSystem.IO;
using OpenWrap.Build;
>>>>>>> f73f0dce227b51986ff52ba6fb3db3b2a48c748f
using OpenWrap.Build.BuildEngines;
using OpenWrap.Repositories;
using OpenWrap.Services;

namespace OpenWrap.Commands.Wrap
{
    [Command(Noun = "Wrap", Verb = "Build")]
    public class BuildWrapCommand : AbstractCommand
    {
<<<<<<< HEAD
        protected IEnvironment Environment { get { return WrapServices.GetService<IEnvironment>(); } }
        protected IFileSystem FileSystem { get { return WrapServices.GetService<IFileSystem>(); } }

        [CommandInput]
        public string PackageName { get; set; }

        public override IEnumerable<ICommandOutput> Execute()
=======
        [CommandInput]
        public string Name { get; set; }

        protected IEnvironment Environment
>>>>>>> f73f0dce227b51986ff52ba6fb3db3b2a48c748f
        {
            get { return WrapServices.GetService<IEnvironment>(); }
        }

        protected IFileSystem FileSystem
        {
            get { return WrapServices.GetService<IFileSystem>(); }
        }

        public override IEnumerable<ICommandOutput> Execute()
        {
            return Either(NoDescriptorFound).Or(Build());
        }

        IEnumerable<ICommandOutput> Build()
        {
<<<<<<< HEAD
            var buildFiles = new List<FileBuildResult>();
            var commandLine = Environment.Descriptor.BuildCommand;
            if (commandLine == null)
             foreach(var t in new ConventionMSBuildEngine(Environment).Build())
             {
                 if (t is TextBuildResult)
                     yield return new GenericMessage(((TextBuildResult)t).Text);
                 else if (t is FileBuildResult)
                     buildFiles.Add((FileBuildResult)t);
             }
            if (buildFiles.Count > 0)
            {
                PackagePackager.CreatePackage(
                    Environment.CurrentDirectory.GetFile(PackageName + ".wrap"),
                    buildFiles.GroupBy(x=>x.ExportName, x=> FileSystem.GetFile(x.Path.FullPath)));
            }
        }

        IEnumerable<ICommandOutput> BuildWithConventionalMsBuild()
=======
            var packageName = Name ?? Environment.Descriptor.Name;
            var buildFiles = new List<FileBuildResult>();
            var commandLine = Environment.Descriptor.BuildCommand;
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
                PackagePackager.CreatePackage(
                        Environment.CurrentDirectory.GetFile(packageName + "-" + version + ".wrap"),
                        version,
                        buildFiles.GroupBy(x => x.ExportName, x => FileSystem.GetFile(x.Path.FullPath)));
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
>>>>>>> f73f0dce227b51986ff52ba6fb3db3b2a48c748f
        {
            var versionFile = Environment.CurrentDirectory.GetFile("version");
            if (versionFile.Exists)
                using (var stream = versionFile.OpenRead())
                using (var streamReader = new StreamReader(stream, Encoding.UTF8))
                    return streamReader.ReadLine();
            return null;
        }

        ICommandOutput NoDescriptorFound()
        {
            return Environment.Descriptor == null ? new GenericError("Could not find a wrap descriptor. Are you in a project directory?") : null;
        }
    }
}