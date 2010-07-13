using System;
using System.IO;
using OpenWrap.IO.FileSystem.Local;

namespace OpenWrap.IO.FileSystem.InMemory
{
    public class InMemoryFile : IFile
    {
        public InMemoryFile(string filePath)
        {
            Path = new LocalPath(filePath);
            Name = System.IO.Path.GetFileName(filePath);
            NameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(filePath);
            Stream = new NonDisposableStream(new MemoryStream());
            LastModifiedTimeUtc = DateTime.Now;
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

        public DateTime? LastModifiedTimeUtc
        {
            get; private set;
        }

        public Stream Open(FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
        {
            if (fileMode == FileMode.Create)
            {
                Stream = new NonDisposableStream(new MemoryStream());
            }
            return Stream;
        }
    }
}