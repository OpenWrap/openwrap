using System.Diagnostics;
using System.IO;

namespace OpenWrap.Build.PackageBuilders
{
    public class ProcessWrapper : IProcess
    {
        Process _process;

        public ProcessWrapper(Process process)
        {
            _process = process;
        }

        public StreamReader StandardOutput
        {
            get { return _process.StandardOutput; }
        }

        public int ExitCode
        {
            get { return _process.ExitCode; }
        }

        public void WaitForExit()
        {
            _process.WaitForExit();
        }

        public void SetEnvironmentVariable(string key, string value)
        {
            _process.StartInfo.EnvironmentVariables[key] = value;
        }

        public bool Start()
        {
            return _process.Start();
        }
    }
}