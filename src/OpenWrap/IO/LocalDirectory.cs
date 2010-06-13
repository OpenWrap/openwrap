using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenWrap.IO
{
    public class LocalDirectory : IDirectory
    {
        readonly DirectoryInfo _di;

        public LocalDirectory(DirectoryInfo directory)
        {
            _di = directory;
        }

        public LocalDirectory(string directoryPath)
        {
            _di = new DirectoryInfo(directoryPath);
        }

        public bool Exists
        {
            get { return _di.Exists; }
        }

        public IFileSystem FileSystem
        {
            get { return IO.FileSystem.Local; }
        }

        public string Name
        {
            get { return _di.Name; }
        }

        public IDirectory Parent
        {
            get { return new LocalDirectory(_di.Parent); }
        }

        public IPath Path
        {
            get { return new LocalPath(_di.FullName); }
        }

        public void Add(IFile file)
        {
            File.Copy(file.Path.FullPath, System.IO.Path.Combine(_di.FullName, file.Name));
        }

        public IEnumerable<IDirectory> Directories(string filter)
        {
            return _di.GetDirectories(filter).Select(x => (IDirectory)new LocalDirectory(x));
        }

        public IEnumerable<IFile> Files()
        {
            return _di.GetFiles().Select(x => (IFile)new LocalFile(x.FullName));
        }

        public IEnumerable<IFile> Files(string filter)
        {
            return _di.GetFiles(filter).Select(x => (IFile)new LocalFile(x.FullName));
        }

        public IDirectory GetDirectory(string directoryPath)
        {
            return new LocalDirectory(System.IO.Path.Combine(_di.FullName, directoryPath));
        }

        public IFile GetFile(string fileName)
        {
            return new LocalFile(System.IO.Path.Combine(_di.FullName, fileName));
        }

        IEnumerable<IDirectory> IDirectory.Directories()
        {
            return _di.GetDirectories().Select(x => (IDirectory)new LocalDirectory(x));
        }

        public void Delete()
        {
            if (_di.Exists)
                _di.Delete(true);
        }

        public IDirectory Create()
        {
            _di.Create();
            return this;
        }
    }
}