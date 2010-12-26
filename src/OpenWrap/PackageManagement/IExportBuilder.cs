using System.Collections.Generic;
using OpenWrap.Runtime;

namespace OpenWrap.PackageManagement
{
    public interface IExportBuilder
    {
        string ExportName { get; }
        bool CanProcessExport(string exportName);
        IExport ProcessExports(IEnumerable<IExport> exports, ExecutionEnvironment environment);
    }
}