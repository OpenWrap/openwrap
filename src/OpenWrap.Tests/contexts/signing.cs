using System;
using System.IO;
using System.Reflection;
using Mono.Cecil;
using Mono.Security;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.InMemory;
using OpenFileSystem.IO.FileSystems.Local;
using OpenWrap.Resources;
using OpenWrap.Testing;
using OpenWrap.Reflection;

namespace Tests.contexts
{
    public class signing : context
    {
        ITemporaryDirectory Dir;
        IFileSystem FileSystem;
        IFile AssemblyFile;
        StrongNameKeyPair Key;
        IFile SourceAssemblyFile;

        public signing()
        {
            FileSystem = LocalFileSystem.Instance;
            Dir = FileSystem.CreateTempDirectory();
        }

        protected void given_key(byte[] key)
        {
            Key = new StrongNameKeyPair(key);
        }

        protected void when_signing()
        {
            SourceAssemblyFile.Sign(AssemblyFile, Key);
        }

        protected void given_assembly_of<T>(string fileName)
        {

            SourceAssemblyFile = Dir.GetFile("old.dll");
            using(var destination = SourceAssemblyFile.OpenWrite())
            using (var source = LocalFileSystem.Instance.GetFile(typeof(T).Assembly.Location).OpenRead())
                source.CopyTo(destination);
            
            AssemblyFile = Dir.GetFile(fileName);
        }
        protected void then_assembly_signing_is_valid()
        {
            using (var assemblyStream = AssemblyFile.OpenRead())
                new StrongName(Key.PublicKey).Verify(assemblyStream).ShouldBeTrue();
        }
    }
}