using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SysPath = System.IO.Path;

namespace OpenWrap.IO
{
    public class InMemoryDirectory : IDirectory, IEquatable<IDirectory>
    {


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
            if (!(obj is IDirectory)) return false;
            return Equals((IDirectory)obj);
        }

        public override int GetHashCode()
        {
            return Path.FullPath.GetHashCode();
        }

        public InMemoryDirectory(string directoryPath, params IFileSystemItem[] children)
        {
            Path = new LocalPath(directoryPath);

            ChildDirectories = new List<InMemoryDirectory>();
            ChildFiles = new List<InMemoryFile>();

            foreach(var childDirectory in children.OfType<InMemoryDirectory>())
            {
                childDirectory.Parent = this;
                childDirectory.Create();
                ChildDirectories.Add(childDirectory);
            }
            foreach(var childFile in children.OfType<InMemoryFile>())
            {
                childFile.Parent = this;
                childFile.Path = Path.Combine(childFile.Path.FullPath);
                childFile.Create();
                ChildFiles.Add(childFile);
            }
        }

        public IDirectory Create()
        {
            Exists = true;
            if (Parent != null && !Parent.Exists)
                Parent.Create();
            return this;
        }

        public IPath Path
        {
            get;
            private set;
        }

        public IDirectory Parent
        {
            get;
            set;
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

        public IDirectory GetDirectory(string directoryName)
        {
            if (System.IO.Path.IsPathRooted(directoryName))
                return FileSystem.GetDirectory(directoryName);

            var inMemoryDirectory =
                ChildDirectories.FirstOrDefault(x => x.Name == directoryName);


            if (inMemoryDirectory == null)
            {
                inMemoryDirectory = new InMemoryDirectory(SysPath.Combine(Path.FullPath, directoryName))
                {
                    Parent = this
                };
                ChildDirectories.Add(inMemoryDirectory);
            }


            return inMemoryDirectory;
        }

        public IFile GetFile(string fileName)
        {
            var file = ChildFiles.FirstOrDefault(x => x.Name.Equals(fileName, StringComparison.OrdinalIgnoreCase));
            if (file == null)
            {
                file = new InMemoryFile(Path.Combine(fileName).FullPath) { Parent = this };
                ChildFiles.Add(file);
            }
            return file;
        }

        public IEnumerable<IFile> Files()
        {
            return ChildFiles.Cast<IFile>();
        }

        public List<InMemoryFile> ChildFiles { get; set; }

        public IEnumerable<IDirectory> Directories()
        {
            return ChildDirectories.Cast<IDirectory>();
        }

        public List<InMemoryDirectory> ChildDirectories { get; set; }

        public IEnumerable<IFile> Files(string filter)
        {
            var filterRegex = filter.Wildcard();
            return ChildFiles.Cast<IFile>().Where(x => filterRegex.IsMatch(x.Name));
        }

        public IEnumerable<IDirectory> Directories(string filter)
        {
            var filterRegex = filter.Wildcard();
            return ChildDirectories.Cast<IDirectory>().Where(x => filterRegex.IsMatch(x.Name));
        }

        public void Add(IFile file)
        {
            ChildFiles.Add((InMemoryFile)file);
        }
    }
}