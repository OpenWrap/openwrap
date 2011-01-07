using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using OpenFileSystem.IO;
using OpenWrap.PackageManagement;
using OpenWrap.PackageManagement.Exporters;
using OpenWrap.PackageModel;
using OpenWrap.Runtime;
using TestDriven.Framework;

namespace OpenWrap.Testing
{
    public class TestRunnerManager
    {
        readonly IFileSystem _fileSystem;
        readonly IEnvironment _environment;
        readonly IPackageResolver _resolver;
        IPackageExporter _exporter;

        public TestRunnerManager(IFileSystem fileSystem, IEnvironment environment, IPackageResolver resolver, IPackageExporter exporter)
        {
            _fileSystem = fileSystem;
            _environment = environment;
            _resolver = resolver;
            _exporter = exporter;
        }

        public IEnumerable<KeyValuePair<string, bool?>> ExecuteAllTests(ExecutionEnvironment environment, IPackage package)
        {
            var descriptor = new PackageDescriptor();
            descriptor.Dependencies.Add(new PackageDependencyBuilder(Guid.NewGuid().ToString()).Name(package.Name).VersionVertex(new EqualVersionVertex(package.Version)));

            var assemblyReferences = _resolver.GetAssemblyReferences(false, environment, descriptor, package.Source, _environment.ProjectRepository, _environment.SystemRepository);
            var runners = GetTestRunners(environment, assemblyReferences);

            var export = package.GetExport("tests", environment);
            if (export == null) return Enumerable.Empty<KeyValuePair<string, bool?>>();
            var testAssemblies = from item in export.Items
                                 where item.FullPath.EndsWithNoCase(".dll")
                                 select item.FullPath;
            return from runner in runners
                   from asm in testAssemblies
                   from result in runner.ExecuteTests(assemblyReferences.Select(x => x.FullPath).ToList(), testAssemblies)
                   select result;
        }

        IEnumerable<TestRunner> GetTestRunners(ExecutionEnvironment environment, IEnumerable<IAssemblyReferenceExportItem> assemblyReferenceExportItems)
        {
            var referencedAssemblies = assemblyReferenceExportItems;
            return from assembly in referencedAssemblies
                   let tdnet = _fileSystem.GetFile(assembly.FullPath + ".tdnet")
                   where tdnet.Exists
                   let runner = LoadRunner(tdnet)
                   where runner != null
                   select runner;
        }

        TestRunner LoadRunner(IFile tdnet)
        {
            using (var stream = tdnet.OpenRead())
            using (var reader = new XmlTextReader(stream))
            {
                var xmlDoc = XDocument.Load(reader);
                var friendlyName = GetChildNode(xmlDoc, "FriendlyName");
                var typeName = GetChildNode(xmlDoc, "TypeName");
                var assemblyPath = GetChildNode(xmlDoc, "AssemblyPath");
                if (typeName != null && assemblyPath != null)
                    return new TestRunner(tdnet.Parent.Path.Combine(assemblyPath), typeName);
            }
            return null;
        }

        string GetChildNode(XDocument xmlDoc, string nodeName)
        {
            return xmlDoc.Descendants(nodeName).Select(x => x.Value).FirstOrDefault() ?? string.Empty;
        }
    }
    public class TestRunner
    {
        readonly string _runnerAssemblyPath;
        readonly string _runnerTypeName;

        public TestRunner(string runnerAssemblyPath, string runnerTypeName)
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
            proxy.Preload(assemblyPaths.Concat(assembliesToTest).ToArray());
            try
            {
                return proxy.RunTests(_runnerAssemblyPath, _runnerTypeName, assembliesToTest);
            }
            finally
            {
                AppDomain.Unload(appDomain);

                System.IO.Directory.Delete(dirs, true);
            }
        }

    }
    public class TestRunnerProxy : MarshalByRefObject
    {
        //Dictionary<AssemblyName, string> _assemblies;
        string _tempDirectory;
        public void SetTempDirectory(string path)
        {
            _tempDirectory = path;
        }
        public void Preload(IEnumerable<string> assemblyPaths)
        {

            //foreach (var asm in assemblyPaths)
            //{
            //    var newFilePath = System.IO.Path.Combine(_tempDirectory, System.IO.Path.GetFileName(asm));
            //    try
            //    {
            //        Assembly.LoadFrom(newFilePath);
            //    }
            //    catch
            //    {
            //    }
            //}
        }
        public override object InitializeLifetimeService()
        {
            return null;
        }
        public IEnumerable<KeyValuePair<string, bool?>> RunTests(string runnerAssemblyPath, string runnerTypeName, IEnumerable<string> assembliesToTest)
        {
            return RunTestsCore(runnerAssemblyPath, runnerTypeName, assembliesToTest).ToArray();
        }

        IEnumerable<KeyValuePair<string, bool?>> RunTestsCore(string runnerAssemblyPath, string runnerTypeName, IEnumerable<string> assembliesToTest)
        {
            var runnerAssembly = Assembly.LoadFrom(runnerAssemblyPath);
            var runnerType = runnerAssembly.GetType(runnerTypeName);

            var runner = Activator.CreateInstance(runnerType) as ITestRunner;
            if (runner == null)
                yield break;
            var testListener = new TestListener();

            foreach (var asm in assembliesToTest)
            {
                bool success = true;
                try
                {
                    var assemblyToTest = Assembly.LoadFrom(System.IO.Path.Combine(_tempDirectory, System.IO.Path.GetFileName(asm)));
                    runner.RunAssembly(testListener, assemblyToTest);
                }
                catch
                {
                    success = false;
                }
                if (!success)
                {
                    yield return new KeyValuePair<string, bool?>("An error occured while executing tests.", false);
                    continue;
                }
                foreach (var result in testListener.Results)
                    yield return new KeyValuePair<string, bool?>(ToMessage(result), ToSuccess(result));
            }
        }

        bool? ToSuccess(TestResult result)
        {
            if (result.State == TestState.Failed) return false;
            if (result.State == TestState.Ignored) return null;
            return true;
        }

        string ToMessage(TestResult result)
        {
            
            return string.Format("\t- Testing {0} {1}", result.Name, result.State);
        }

        //Assembly ResolveAssemblies(object sender, ResolveEventArgs args)
        //{
        //    var simpleName = new AssemblyName(args.Name).Name;
        //    var assemblyName = _assemblies.Keys.Where(x => x.Name == simpleName).FirstOrDefault();
        //    if (assemblyName != null)
        //    {
        //        Assembly asm = null;
        //        try
        //        {
        //            asm = Assembly.LoadFrom(_assemblies[assemblyName]);
        //        }
        //        catch
        //        {

        //        }
        //        return asm ?? ResolveAssemblyFromExistingPaths(simpleName);
        //    }
        //    return null;
        //}

        Assembly ResolveAssemblyFromExistingPaths(string simpleName)
        {
            return Assembly.Load(simpleName);
        }

        public class TestListener : ITestListener
        {
            public TestListener()
            {
                Results = new List<TestResult>();
            }
            public void WriteLine(string text, Category category)
            {
            }

            public void TestFinished(TestResult summary)
            {
                Results.Add(summary);
            }

            public List<TestResult> Results { get; set; }

            public void TestResultsUrl(string resultsUrl)
            {
            }
        }


    }
}
