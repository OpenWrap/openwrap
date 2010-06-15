using System.IO;

namespace OpenWrap.IO
{
    public class InMemoryFile : IFile
    {
        public InMemoryFile(string filePath)
        {
            Path = new LocalPath(filePath);
            Name = System.IO.Path.GetFileName(filePath);
            NameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(filePath);

        }
        public Stream Stream { get; set; }
        public IFile Create()
        {
            Exists = true;
            return this;
        }

        public IPath Path { get; private set; }
        public IDirectory Parent
        {
            get
            {
                return new InMemoryDirectory(System.IO.Path.GetDirectoryName(Path.FullPath))
                {
                    FileSystem = this.FileSystem
                };
            }
        }

        public IFileSystem FileSystem { get; set; }
        public bool Exists { get; set; }
        public string Name { get; private set; }
        public void Delete()
        {
            Exists = false;
        }

        public string NameWithoutExtension { get; private set; }
        public Stream Open(FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
        {
            return Stream;
        }
    }
}