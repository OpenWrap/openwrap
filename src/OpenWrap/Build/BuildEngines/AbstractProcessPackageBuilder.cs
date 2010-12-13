using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenFileSystem.IO;

namespace OpenWrap.Build.BuildEngines
{
    public abstract class AbstractProcessPackageBuilder : IPackageBuilder
    {
        protected IFileSystem _fileSystem;
        protected IEnvironment _environment;
        protected BuiltInstructionParser InstructionParser { get; set; }
        protected abstract string ExecutablePath { get; }

        protected AbstractProcessPackageBuilder(IFileSystem fileSystem, IEnvironment environment)
        {
            _fileSystem = fileSystem;
            _environment = environment;
            InstructionParser = new BuiltInstructionParser();
        }

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
                var parsed = InstructionParser.Parse(line);
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