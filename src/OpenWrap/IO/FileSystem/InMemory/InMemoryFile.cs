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
            CreateNewStream();
            LastModifiedTimeUtc = DateTime.Now;
        }

        void CreateNewStream()
        {
            Stream = new NonDisposableStream(new MemoryStream());
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
            get;
            set;
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
            get;
            private set;
        }

        public Stream Open(FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
        {
            ValidateFileMode(fileMode);
            return Stream;
        }

        void ValidateFileMode(FileMode fileMode)
        {
            if (Exists)
            {
                switch (fileMode)
                {
                    case FileMode.CreateNew:
                        throw new IOException("File already exists.");
                    case FileMode.Create:
                    case FileMode.Truncate:
                        CreateNewStream();
                        break;
                    case FileMode.Append:
                        Stream.Position = Stream.Length;
                        break;
                }
            }
            else
            {
                switch (fileMode)
                {
                    case FileMode.Append:
                    case FileMode.Create:
                    case FileMode.CreateNew:
                    case FileMode.OpenOrCreate:
                        Exists = true;
                        CreateNewStream();
                        break;
                    case FileMode.Open:
                    case FileMode.Truncate:
                        throw new FileNotFoundException();
                }
            }
        }
    }
}