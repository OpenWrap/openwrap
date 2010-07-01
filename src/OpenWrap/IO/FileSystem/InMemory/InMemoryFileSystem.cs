using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenWrap.IO.FileSystem.Local;

namespace OpenWrap.IO.FileSystem.InMemory
{
    public class InMemoryFileSystem : IFileSystem
    {
        public Dictionary<string, InMemoryDirectory> Directories { get; private set; }

        public InMemoryFileSystem(params InMemoryDirectory[] childDirectories)
        {
            Directories = new Dictionary<string, InMemoryDirectory>();
            CurrentDirectory = @"c:\";
            
            foreach(var directory in childDirectories)
            {
                var root = GetRoot(directory.Path.Segments.First());
                foreach(var segment in directory.Path.Segments
                    .Skip(1)
                    .Take(directory.Path.Segments.Count()-2))
                {
                    root = (InMemoryDirectory)root.GetDirectory(segment);
                }
                directory.Parent = root;
                root.ChildDirectories.Add(directory);
                directory.Create();
            }

        }
        
        InMemoryDirectory GetRoot(string path)
        {
            InMemoryDirectory directory;
            if (!Directories.TryGetValue(path, out directory))
            {
                Directories.Add(path, directory = new InMemoryDirectory(path));
            }
            return directory;
        }
        public IDirectory GetDirectory(string directoryPath)
        {
            var resolvedDirectoryPath = Path.GetFullPath(Path.Combine(CurrentDirectory,directoryPath));
            var pathSegments = new LocalPath(resolvedDirectoryPath).Segments;
            return pathSegments
                .Skip(1)
                .Aggregate((IDirectory)GetRoot(pathSegments.First()),
                    (current, segment) => current.GetDirectory(segment));
        }

        public IFile GetFile(string filePath)
        {
            var resolviedFilePath = Path.GetFullPath(Path.Combine(CurrentDirectory, filePath));
            var pathSegments = new LocalPath(resolviedFilePath).Segments;
            var ownerFolder = pathSegments
                .Skip(1).Take(pathSegments.Count()-2)
                .Aggregate((IDirectory)GetRoot(pathSegments.First()),
                    (current, segment) => current.GetDirectory(segment));
            return ownerFolder.GetFile(pathSegments.Last());
        }

        bool DirectoryExists(string resolvedDirectoryPath)
        {
            throw new InvalidOperationException();

        }

        public IPath GetPath(string path)
        {
            return new LocalPath(path);
        }

        public ITemporaryDirectory CreateTempDirectory()
        {
            return new InMemoryTemporaryDirectory(Path.Combine(@"c:\temporary", Path.GetRandomFileName()))
            {
                Exists = true,
                FileSystem = this
            };
        }

        public IDirectory CreateDirectory(string path)
        {
            return new InMemoryDirectory(path)
            {
                FileSystem = this,
                Exists = true
            };
        }

        public ITemporaryFile CreateTempFile()
        {
            return new InMemoryTemporaryFile(Path.Combine(@"c:\temporary", Path.GetRandomFileName()))
            {
                Exists = true,
                FileSystem = this
            };
        }

        public IDirectory GetTempDirectory()
        {
            return new InMemoryTemporaryDirectory(Path.GetTempPath())
            {
                Exists = true,
                FileSystem = this
            
            
            };
        }

        public string CurrentDirectory { get; set; }
    }
}