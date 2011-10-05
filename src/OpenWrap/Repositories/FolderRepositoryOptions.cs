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
        Default = 0
    }
}