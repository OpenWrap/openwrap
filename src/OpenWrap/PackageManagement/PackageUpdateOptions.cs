using System;

namespace OpenWrap.PackageManagement
{
    [Flags]
    public enum PackageUpdateOptions
    {
        Recurse = 1,
        Default = Recurse
    }
}