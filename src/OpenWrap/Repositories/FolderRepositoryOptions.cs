using System;

namespace OpenWrap.Repositories
{
    [Flags]
    public enum FolderRepositoryOptions
    {
        UseSymLinks = 1,
        AnchoringEnabled = 2,
        Default = UseSymLinks
    }
}