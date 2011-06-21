using System;
using System.IO;
using System.Reflection;
using Mono.Cecil;
using OpenFileSystem.IO;

namespace OpenWrap.Reflection
{
    public static class AssemblyFiddler
    {
        public static void Sign(this IFile sourceAssembly, IFile destinationAssembly, StrongNameKeyPair key, Version version = null)
        {
            using(var sourceStream = sourceAssembly.OpenRead())
            {
                var assembly = AssemblyDefinition.ReadAssembly(sourceStream);
                var name = assembly.Name;
                name.HashAlgorithm = AssemblyHashAlgorithm.SHA1;
                name.PublicKey = key.PublicKey;
                if (version != null)
                    name.Version = version;
                assembly.Name.HasPublicKey = true;
                using (var destinationStream = destinationAssembly.Open(FileMode.Create, FileAccess.ReadWrite, FileShare.None))
                    assembly.Write(destinationStream, new WriterParameters() { StrongNameKeyPair = key });
            }
        }
    }
}