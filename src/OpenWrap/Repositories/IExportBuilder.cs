using System.Collections.Generic;
using OpenRasta.Wrap.Dependencies;
using OpenRasta.Wrap.Resources;

namespace OpenRasta.Wrap.Repositories
{
    public interface IExportBuilder
    {
        string ExportName { get; }
        bool CanProcessExport(string exportName);
        IWrapExport ProcessExports(IEnumerable<IWrapExport> exports, WrapRuntimeEnvironment environment);
    }
}