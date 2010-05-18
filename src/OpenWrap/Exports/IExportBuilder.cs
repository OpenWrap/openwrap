using System.Collections.Generic;
using OpenWrap.Dependencies;

namespace OpenWrap.Exports
{
    public interface IExportBuilder
    {
        string ExportName { get; }
        bool CanProcessExport(string exportName);
        IExport ProcessExports(IEnumerable<IExport> exports, WrapRuntimeEnvironment environment);
    }
}