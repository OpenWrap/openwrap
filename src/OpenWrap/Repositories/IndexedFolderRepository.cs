using System;
using OpenFileSystem.IO;
using OpenWrap.Repositories.Http;

namespace OpenWrap.Repositories
{
    public class IndexedFolderRepository : HttpRepository, ISupportNuking
    {
        readonly NetworkShareNavigator _navigator;

        public IndexedFolderRepository(string repositoryName, IDirectory directory)
            : base(directory.FileSystem, repositoryName, new NetworkShareNavigator(directory))
        {
            _navigator = Navigator as NetworkShareNavigator;
        }

        public void Nuke(IPackageInfo packageInfo)
        {
            _navigator.Nuke(packageInfo);
        }
        
    }
}