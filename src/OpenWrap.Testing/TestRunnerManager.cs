using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using OpenFileSystem.IO;
using OpenWrap.Collections;
using OpenWrap.PackageManagement;
using OpenWrap.PackageManagement.Exporters;
using OpenWrap.PackageModel;
using OpenWrap.Runtime;

namespace OpenWrap.Testing
{
    public interface ITestRunnerFactory
    {
        IEnumerable<ITestRunner> GetTestRunners(ExecutionEnvironment environment, IEnumerable<IAssemblyReferenceExportItem> allReferencedAssemblies);
    }

    public class TdnetTestRunnerFactory : ITestRunnerFactory
    {
        IFileSystem _fileSystem;

        public TdnetTestRunnerFactory(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public IEnumerable<ITestRunner> GetTestRunners(ExecutionEnvironment environment, IEnumerable<IAssemblyReferenceExportItem> allReferencedAssemblies)
        {
            var referencedAssemblies = allReferencedAssemblies;
            return from assembly in referencedAssemblies
                   let tdnet = _fileSystem.GetFile(assembly.FullPath + ".tdnet")
                   where tdnet.Exists
                   let runner = LoadRunner(tdnet)
                   where runner != null
                   select runner;
        }

        ITestRunner LoadRunner(IFile tdnet)
        {
            using (var stream = tdnet.OpenRead())
            using (var reader = new XmlTextReader(stream))
            {
                var xmlDoc = XDocument.Load(reader);
                var friendlyName = GetChildNode(xmlDoc, "FriendlyName");
                var typeName = GetChildNode(xmlDoc, "TypeName");
                var assemblyPath = GetChildNode(xmlDoc, "AssemblyPath");
                if (typeName != null && assemblyPath != null)
                    return new AppDomainTestRunner(tdnet.Parent.Path.Combine(assemblyPath), typeName);
            }
            return null;
        }

        string GetChildNode(XDocument xmlDoc, string nodeName)
        {
            return xmlDoc.Descendants(nodeName).Select(x => x.Value).FirstOrDefault() ?? string.Empty;
        }
    }
    public class TestRunnerManager
    {
        readonly IEnumerable<ITestRunnerFactory> _factories;
        readonly IEnvironment _environment;
        readonly IPackageResolver _resolver;

        public TestRunnerManager(IEnumerable<ITestRunnerFactory> factories, IEnvironment environment, IPackageResolver resolver)
        {
            _factories = factories;
            _environment = environment;
            _resolver = resolver;
        }

        public IEnumerable<KeyValuePair<string, bool?>> ExecuteAllTests(ExecutionEnvironment environment, IPackage package)
        {
            var descriptor = new PackageDescriptor();
            descriptor.Dependencies.Add(new PackageDependencyBuilder(Guid.NewGuid().ToString()).Name(package.Name).VersionVertex(new EqualVersionVertex(package.Version)));
            
            var allAssemblyReferences = _resolver.GetAssemblyReferences(false, environment, descriptor, package.Source, _environment.ProjectRepository, _environment.SystemRepository);

            var runners = _factories.SelectMany(x=>x.GetTestRunners(environment, allAssemblyReferences)).NotNull();

            var export = package.GetExport("tests", environment);

            if (export == null) return Enumerable.Empty<KeyValuePair<string, bool?>>();

            var testAssemblies = from item in export.Items
                                 where item.FullPath.EndsWithNoCase(".dll")
                                 select item.FullPath;
            return from runner in runners
                   from asm in testAssemblies
                   from result in runner.ExecuteTests(allAssemblyReferences.Select(x => x.FullPath).ToList(), testAssemblies)
                   select result;
        }

    }
}
