using System;
using OpenWrap.Repositories.Http;
using OpenFileSystem.IO;

namespace OpenWrap.Repositories
{
    public class IndexedFolderRepository : HttpRepository
    {
        public IndexedFolderRepository(string repositoryName, IDirectory directory)
            : base(directory.FileSystem, repositoryName, new NetworkShareNavigator(directory))
        {
        }

        
    }
}