using System;
using OpenWrap.Repositories.Http;
using OpenFileSystem.IO;

namespace OpenWrap.Repositories
{
    public class IndexedFolderRepository : HttpRepository
    {
        public IndexedFolderRepository(IDirectory directory)
            : base(directory.FileSystem, new NetworkShareNavigator(directory))
        {
        }

        
    }
}