using System;
using System.Collections.Generic;
using System.IO;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.InMemory;
using OpenWrap.PackageModel;
using OpenWrap.PackageModel.Serialization;

namespace OpenWrap.contexts
{
    public class descriptor_readers : OpenWrap.Testing.context
    {
        protected InMemoryFileSystem FileSystem;
        protected IDictionary<string, FileBased<IPackageDescriptor>> Descriptors;

        public descriptor_readers()
        {
            FileSystem = new InMemoryFileSystem();

        }
        protected void given_descriptor(string fileName, params string[] lines)
        {
            using (var writer = new StreamWriter(FileSystem.GetCurrentDirectory().GetFile(fileName).OpenWrite()))
                writer.Write(lines.Join("\r\n"));
        }
        protected void when_reading_all()
        {
            Descriptors = new PackageDescriptorReader().ReadAll(FileSystem.GetCurrentDirectory());
        }
    }
}