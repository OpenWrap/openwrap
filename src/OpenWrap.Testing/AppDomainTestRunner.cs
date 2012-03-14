using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenWrap.Testing
{
    public class AppDomainTestRunner : ITestRunner
    {
        readonly string _runnerAssemblyPath;
        readonly string _runnerTypeName;

        public AppDomainTestRunner(string runnerAssemblyPath, string runnerTypeName)
        {
            _runnerAssemblyPath = runnerAssemblyPath;
            _runnerTypeName = runnerTypeName;
        }

        public IEnumerable<KeyValuePair<string, bool?>> ExecuteTests(IEnumerable<string> assemblyPaths, IEnumerable<string> assembliesToTest)
        {
            var dirs = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString());

            System.IO.Directory.CreateDirectory(dirs);
            foreach (var asm in assemblyPaths.Concat(assembliesToTest))
            {
                var newFilePath = System.IO.Path.Combine(dirs, System.IO.Path.GetFileName(asm));
                System.IO.File.Copy(asm, newFilePath, true);
            }
            assembliesToTest = assembliesToTest.Select(x => System.IO.Path.Combine(dirs, System.IO.Path.GetFileName(x))).ToArray();
            var appDomain = AppDomain.CreateDomain("OpenWrap test runner", null, new AppDomainSetup()
            {
                    PrivateBinPath = dirs,
                    ApplicationBase = dirs
            });

            var proxy = (TestRunnerProxy)appDomain.CreateInstanceFromAndUnwrap(typeof(TestRunnerProxy).Assembly.Location, typeof(TestRunnerProxy).FullName);
            proxy.SetTempDirectory(dirs);
            
            try
            {
                return proxy.RunTests(_runnerAssemblyPath, _runnerTypeName, assembliesToTest);
            }
            finally
            {
                try
                {
                    AppDomain.Unload(appDomain);

                    System.IO.Directory.Delete(dirs, true);
                }
                catch
                {
                }
            }
        }

    }
}