using System.IO;

namespace OpenWrap.Build.PackageBuilders
{
    public interface IProcess
    {
        StreamReader StandardOutput { get; }
        int ExitCode { get; }
        bool Start();
        void WaitForExit();
        void SetEnvironmentVariable(string key, string value);
    }
}