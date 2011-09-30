using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenFileSystem.IO;

namespace OpenWrap.Build.PackageBuilders
{
    public abstract class AbstractProcessPackageBuilder : IPackageBuilder
    {
        protected IFileSystem _fileSystem;

        protected AbstractProcessPackageBuilder(IFileSystem fileSystem, IFileBuildResultParser fileBuildResultParser)
        {
            _fileSystem = fileSystem;
            FileBuildParser = fileBuildResultParser;
        }

        protected abstract string ExecutablePath { get; }
        protected IFileBuildResultParser FileBuildParser { get; private set; }

        public abstract IEnumerable<BuildResult> Build();

        protected virtual IProcess CreateProcess(string arguments)
        {
            return new ProcessWrapper(new Process
            {
                    StartInfo = new ProcessStartInfo
                    {
                            RedirectStandardOutput = true,
                            FileName = ExecutablePath,
                            Arguments = arguments,
                            UseShellExecute = false
                    }
            });
        }

        protected IEnumerable<BuildResult> ExecuteEngine(IProcess process)
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