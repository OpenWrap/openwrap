using System;

namespace OpenWrap.Repositories
{
    [Flags]
    public enum FolderRepositoryOptions
    {
        UseSymLinks = 1,
        AnchoringEnabled = 1<<1,
        Default = UseSymLinks
    }
}