using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using OpenWrap.Collections;
using OpenWrap.PackageManagement;
using OpenWrap.PackageManagement.Exporters;
using OpenWrap.PackageManagement.Exporters.Assemblies;
using OpenWrap.PackageModel;
using OpenWrap.Runtime;

namespace OpenWrap.Testing
{
    public class TestRunnerManager : ITestRunnerManager
    {
        readonly IEnumerable<ITestRunnerFactory> _factories;
        readonly IEnvironment _environment;
        readonly IPackageManager _manager;

        public TestRunnerManager(IEnumerable<ITestRunnerFactory> factories, IEnvironment environment, IPackageManager manager)
        {
            _factories = factories;
            _environment = environment;
            _manager = manager;
        }

        public IEnumerable<KeyValuePair<string, bool?>> ExecuteAllTests(ExecutionEnvironment environment, IPackage package)
        {
            var descriptor = new PackageDescriptor();
            descriptor.Dependencies.Add(new PackageDependencyBuilder(Guid.NewGuid().ToString()).Name(package.Name).VersionVertex(new EqualVersionVertex(package.Version)));

            var allAssemblyReferences = _manager.GetProjectAssemblyReferences(descriptor, package.Source, environment, false);

            var runners = _factories.SelectMany(x => x.GetTestRunners(allAssemblyReferences)).NotNull();

            var tests = new DefaultAssemblyExporter("tests").Items<Exports.IAssembly>(package, environment);

            if (tests == null) return Enumerable.Empty<KeyValuePair<string, bool?>>();

            var testAssemblies = from item in tests.SelectMany(x=>x)
                                 where item.File.Extension.Equals(".dll")
                                 select item.File.Path.FullPath;
            return from runner in runners
                   from asm in testAssemblies
                   from result in runner.ExecuteTests(allAssemblyReferences.Select(x => x.File.Path.FullPath).ToList(), testAssemblies)
                   select result;
        }

    }
}
