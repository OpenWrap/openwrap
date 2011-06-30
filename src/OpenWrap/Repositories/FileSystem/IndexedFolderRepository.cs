using OpenFileSystem.IO;
using OpenWrap.PackageModel;
using OpenWrap.Repositories.Http;

namespace OpenWrap.Repositories.FileSystem
{
    public class IndexedFolderRepository : IndexedHttpRepository, ISupportNuking
    {
        readonly FileSystemNavigator _navigator;

        public IndexedFolderRepository(string repositoryName, IDirectory directory)
                : base(directory.FileSystem, repositoryName, new FileSystemNavigator(directory))
        {
            _navigator = Navigator as FileSystemNavigator;
            Directory = directory;
        }
        public override string Type { get { return "indexed-folder"; } }

        public IDirectory Directory { get; private set; }
        public void Nuke(IPackageInfo packageInfo)
        {
            _navigator.Nuke(packageInfo);
        }
    }
}