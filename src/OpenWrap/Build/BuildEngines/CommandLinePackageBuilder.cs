using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenFileSystem.IO;

namespace OpenWrap.Build.BuildEngines
{
    public abstract class CommandLinePackageBuilder : IPackageBuilder
    {
        protected IFileSystem _fileSystem;
        protected IEnvironment _environment;
        protected BuiltInstructionParser InstructionParser { get; set; }
        protected abstract string ExecutablePath { get; }
        protected virtual string ExecutableArgs { get { return string.Empty; } }

        protected CommandLinePackageBuilder(IFileSystem fileSystem, IEnvironment environment)
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

        protected IEnumerable<BuildResult> ExecuteEngine(Process msbuildProcess)
        {
            msbuildProcess.Start();
            var reader = msbuildProcess.StandardOutput;

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
            msbuildProcess.WaitForExit();
            if (msbuildProcess.ExitCode != 0)
                yield return new ErrorBuildResult();
        }
    }
}