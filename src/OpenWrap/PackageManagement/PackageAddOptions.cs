using System;

namespace OpenWrap.PackageManagement
{
    [Flags]
    public enum PackageAddOptions
    {
        UpdateDescriptor = 1,
        Content = 2,
        Anchor = 4,
        Force = 8,
        Test = 16,
        Default = Test | UpdateDescriptor
    }
}