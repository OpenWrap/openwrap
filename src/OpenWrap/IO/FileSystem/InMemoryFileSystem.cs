using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using SysPath = System.IO.Path;

namespace OpenWrap.IO
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
            var resolvedDirectoryPath = SysPath.GetFullPath(SysPath.Combine(CurrentDirectory,directoryPath));
            var pathSegments = new LocalPath(resolvedDirectoryPath).Segments;
            return pathSegments
                .Skip(1)
                .Aggregate((IDirectory)GetRoot(pathSegments.First()),
                    (current, segment) => current.GetDirectory(segment));
        }

        public IFile GetFile(string filePath)
        {
            var resolviedFilePath = SysPath.GetFullPath(SysPath.Combine(CurrentDirectory, filePath));
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
            return new InMemoryTemporaryDirectory(SysPath.Combine(@"c:\temporary", SysPath.GetRandomFileName()))
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
            return new InMemoryTemporaryFile(SysPath.Combine(@"c:\temporary", SysPath.GetRandomFileName()))
            {
                Exists = true,
                FileSystem = this
            };
        }

        public IDirectory GetTempDirectory()
        {
            return new InMemoryTemporaryDirectory(SysPath.GetTempPath())
            {
                Exists = true,
                FileSystem = this
            
            
            };
        }

        public string CurrentDirectory { get; set; }
    }
}