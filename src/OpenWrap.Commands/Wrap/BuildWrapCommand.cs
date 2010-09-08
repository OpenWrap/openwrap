using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using OpenFileSystem.IO;
using OpenWrap.Build.BuildEngines;
using OpenWrap.Repositories;
using OpenWrap.Services;

namespace OpenWrap.Commands.Wrap
{
    [Command(Noun="Wrap", Verb="Build")]
    public class BuildWrapCommand : AbstractCommand
    {
        protected IEnvironment Environment { get { return WrapServices.GetService<IEnvironment>(); } }
        protected IFileSystem FileSystem { get { return WrapServices.GetService<IFileSystem>(); } }

        [CommandInput]
        public string PackageName { get; set; }

        public override IEnumerable<ICommandOutput> Execute()
        {
            return Either(NoDescriptorFound).Or(Build());
        }
        ICommandOutput NoDescriptorFound()
        {
            return Environment.Descriptor == null ? new GenericError("Could not find a wrap descriptor. Are you in a project directory?") : null;
        }
        IEnumerable<ICommandOutput> Build()
        {
            var buildFiles = new List<FileBuildResult>();
            var commandLine = Environment.Descriptor.BuildCommand;
            if (commandLine == null)
             foreach(var t in new ConventionMSBuildEngine(Environment).Build())
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
                PackagePackager.CreatePackage(
                    Environment.CurrentDirectory.GetFile(PackageName + ".wrap"),
                    buildFiles.GroupBy(x=>x.ExportName, x=> FileSystem.GetFile(x.Path.FullPath)));
            }
        }

        IEnumerable<ICommandOutput> BuildWithConventionalMsBuild()
        {
            yield break;
        }
    }
}
