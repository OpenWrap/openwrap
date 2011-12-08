using System;

namespace OpenWrap.PackageManagement
{
    [Flags]
    public enum AssemblyExportFlags
    {
        None,
        ReferencedAssembly,
        RuntimeAssembly
    }
}