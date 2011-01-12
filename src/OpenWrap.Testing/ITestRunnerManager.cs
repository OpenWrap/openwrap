using System.Collections.Generic;
using OpenWrap.PackageModel;
using OpenWrap.Runtime;

namespace OpenWrap.Testing
{
    public interface ITestRunnerManager
    {
        IEnumerable<KeyValuePair<string, bool?>> ExecuteAllTests(ExecutionEnvironment environment, IPackage package);
    }
}