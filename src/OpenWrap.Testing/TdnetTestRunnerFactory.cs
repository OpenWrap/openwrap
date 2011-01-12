using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using OpenFileSystem.IO;
using OpenWrap.PackageManagement.Exporters;

namespace OpenWrap.Testing
{
    public class TdnetTestRunnerFactory : ITestRunnerFactory
    {
        IFileSystem _fileSystem;

        public TdnetTestRunnerFactory(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public IEnumerable<ITestRunner> GetTestRunners(IEnumerable<IAssemblyReferenceExportItem> allReferencedAssemblies)
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
}