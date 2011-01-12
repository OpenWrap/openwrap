using System.Collections.Generic;
using OpenWrap.PackageManagement.Exporters;

namespace OpenWrap.Testing
{
    public interface ITestRunnerFactory
    {
        IEnumerable<ITestRunner> GetTestRunners(IEnumerable<IAssemblyReferenceExportItem> allReferencedAssemblies);
    }
}