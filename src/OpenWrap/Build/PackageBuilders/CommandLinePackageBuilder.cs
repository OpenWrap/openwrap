using System.Collections.Generic;
using System.Linq;
using OpenFileSystem.IO;
using OpenWrap.Runtime;

namespace OpenWrap.Build.PackageBuilders
{
    public class CommandLinePackageBuilder : AbstractProcessPackageBuilder
    {
        public CommandLinePackageBuilder(IFileSystem fileSystem, IEnvironment environment, IFileBuildResultParser resultParser)
            : base(fileSystem, resultParser)
        {

        }
        public IEnumerable<string> Args { get; set; }
        public IEnumerable<string> Path { get; set; }

        protected override string ExecutablePath
        {
            get { return Path.First(); }
        }

        public override IEnumerable<BuildResult> Build()
        {
            return ExecuteEngine(CreateProcess(Args.FirstOrDefault() ?? string.Empty));
        }
    }
}