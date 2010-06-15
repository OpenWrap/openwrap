using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenWrap.IO
{
    public class InMemoryDirectory : IDirectory, IEquatable<IDirectory>
    {
        public InMemoryDirectory()
        {
            
        }

        //public InMemoryDirectory(params IFile)
        //{
            
        //}
        public bool Equals(IDirectory other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Path.FullPath, Path.FullPath);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(IDirectory)) return false;
            return Equals((IDirectory)obj);
        }

        public override int GetHashCode()
        {
            return Path.FullPath.GetHashCode();
        }

        public InMemoryDirectory(string directoryPath)
        {
            Path = new LocalPath(directoryPath);
            
            ChildFiles = new List<IFile>();
        }

        public IDirectory Create()
        {
            Exists = true;
            return this;
        }

        public IPath Path
        {
            get;
            private set;
        }

        public IDirectory Parent
        {
            get
            {
                var parent = new DirectoryInfo(Path.FullPath).FullName;
                return parent != null ? new InMemoryDirectory(parent) : null;
            }
        }

        public IFileSystem FileSystem
        {
            get;
            set;
        }

        public bool Exists
        {
            get;
            set;
        }

        public string Name
        {
            get { return System.IO.Path.GetFileName(Path.FullPath); }
        }

        public void Delete()
        {
            Exists = false;
        }

        public IDirectory GetDirectory(string directoryPath)
        {
            return new InMemoryDirectory(Path.Combine(directoryPath).FullPath);
        }

        public IFile GetFile(string fileName)
        {
            return new InMemoryFile(System.IO.Path.Combine(Path.FullPath, fileName));
        }

        public IEnumerable<IFile> Files()
        {
            return ChildFiles.Cast<IFile>();
        }

        protected List<IFile> ChildFiles { get; set; }

        public IEnumerable<IDirectory> Directories()
        {
            return ChildDirectories.Cast<IDirectory>();
        }

        protected List<InMemoryDirectory> ChildDirectories { get; set; }

        public IEnumerable<IFile> Files(string filter)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IDirectory> Directories(string filter)
        {
            var filterRegex = filter.Wildcard();
            return ChildDirectories.Cast<IDirectory>().Where(x=>filterRegex.IsMatch(x.Name));
        }

        public void Add(IFile file)
        {
            ChildFiles.Add(file);
        }
    }
}