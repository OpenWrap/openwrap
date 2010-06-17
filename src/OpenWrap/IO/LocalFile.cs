using System;
using System.IO;

namespace OpenWrap.IO
{
    public class LocalFile : IFile
    {
        readonly string _filePath;

        public LocalFile(string filePath)
        {
            _filePath = filePath;
            Path = new LocalPath(filePath);
        }

        public bool Exists
        {
            get { return File.Exists(_filePath); }
        }

        public IFileSystem FileSystem
        {
            get { return IO.FileSystem.Local; }
        }

        public string Name
        {
            get { return System.IO.Path.GetFileName(Name); }
        }

        public string NameWithoutExtension
        {
            get { return System.IO.Path.GetFileNameWithoutExtension(_filePath); }
        }

        public DateTime? LastModifiedTimeUtc
        {
            get { return Exists ? new FileInfo(_filePath).LastWriteTimeUtc : (DateTime?)null; }
        }

        public IDirectory Parent
        {
            get
            {
                try
                {
                    var directoryInfo = Directory.GetParent(_filePath);
                    return directoryInfo == null
                               ? null
                               : new LocalDirectory(directoryInfo.FullName);
                }
                catch (DirectoryNotFoundException)
                {
                    return null;
                }
            }
        }

        public IPath Path { get; private set; }

        public Stream Open(FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
        {
            return File.Open(_filePath, fileMode, fileAccess, fileShare);
        }

        public void Delete()
        {
            File.Delete(_filePath);
        }

        public IFile Create()
        {
            // creates the parent if it doesnt exist
            if (!Parent.Exists)
                Parent.Create();

            File.Create(Path.FullPath).Close();
            return this;
        }
    }
}