using System.Collections.Generic;

namespace OpenWrap.Testing
{
    public interface ITestRunner
    {
        IEnumerable<KeyValuePair<string, bool?>> ExecuteTests(IEnumerable<string> assemblyPaths, IEnumerable<string> assembliesToTest);
    }
}