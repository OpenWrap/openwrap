using System.Collections.Generic;
using System.Linq;
using OpenFileSystem.IO;

namespace OpenWrap.Build.BuildEngines
{
    public class CommandLinePackageBuilder : AbstractProcessPackageBuilder
    {
        public IEnumerable<string> Path { get; set; }
        public IEnumerable<string> Args { get; set; }

        protected override string ExecutablePath
        {
            get { return Path.First(); }
        }

        public override IEnumerable<BuildResult> Build()
        {
            return ExecuteEngine(CreateProcess(Args.FirstOrDefault() ?? string.Empty));
        }

        public CommandLinePackageBuilder(IFileSystem fileSystem, IEnvironment environment) : base(fileSystem, environment)
        {
        }
    }
}