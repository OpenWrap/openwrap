using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.InMemory;
using OpenWrap.IO;
using OpenWrap.Repositories.FileSystem;
using OpenWrap.Repositories.Http;
using Tests.contexts;

namespace Tests.Repositories.contexts
{
    public class indexed_folder_repository : repository_factory<IndexedFolderRepositoryFactory, IndexedFolderRepository>
    {
        protected IFileSystem FileSystem;

        public indexed_folder_repository()
            : this(new InMemoryFileSystem())
        {
            
        }
        public indexed_folder_repository(IFileSystem fileSystem)
            : base(_ => new IndexedFolderRepositoryFactory(fileSystem))
        {
            FileSystem = fileSystem;
        }
        protected void given_file(string filePath, string content)
        {
            FileSystem.GetFile(filePath).MustExist().WriteString(content);
        }

        protected void given_directory(string path)
        {
            FileSystem.GetDirectory(path).MustExist();
        }
    }
}