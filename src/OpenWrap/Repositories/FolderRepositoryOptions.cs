using System;

namespace OpenWrap.Repositories
{
    [Flags]
    public enum FolderRepositoryOptions
    {
        UseSymLinks = 1,
        AnchoringEnabled = 2,
        PersistPackageSources = 4,
        PersistPackages = 8,
        SupportLocks = 32,

        Default = 0
    }
}
