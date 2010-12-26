using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenFileSystem.IO;
using OpenWrap.Runtime;

namespace OpenWrap.Build.PackageBuilders
{
    public abstract class AbstractProcessPackageBuilder : IPackageBuilder
    {
        protected IEnvironment _environment;
        protected IFileSystem _fileSystem;

        protected AbstractProcessPackageBuilder(IFileSystem fileSystem, IEnvironment environment, IFileBuildResultParser fileBuildResultParser)
        {
            _fileSystem = fileSystem;
            _environment = environment;
            FileBuildParser = fileBuildResultParser;
        }

        protected abstract string ExecutablePath { get; }
        protected IFileBuildResultParser FileBuildParser { get; private set; }

        public abstract IEnumerable<BuildResult> Build();

        protected Process CreateProcess(string arguments)
        {
            return new Process
            {
                    StartInfo = new ProcessStartInfo
                    {
                            RedirectStandardOutput = true,
                            FileName = ExecutablePath,
                            Arguments = arguments,
                            UseShellExecute = false
                    }
            };
        }

        protected IEnumerable<BuildResult> ExecuteEngine(Process process)
        {
            process.Start();
            var reader = process.StandardOutput;

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var parsed = FileBuildParser.Parse(line);
                if (parsed.Count() > 0)
                    foreach (var m in parsed)
                        yield return m;
                else
                    yield return new TextBuildResult(line);
            }
            process.WaitForExit();
            if (process.ExitCode != 0)
                yield return new ErrorBuildResult();
        }
    }
}