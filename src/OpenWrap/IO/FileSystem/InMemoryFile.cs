using System;
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
            Stream = new MemoryStream();
        }
        public Stream Stream { get; set; }
        public IFile Create()
        {
            Exists = true;
            return this;
        }

        public IPath Path { get; set; }
        public IDirectory Parent
        {
            get; set;
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