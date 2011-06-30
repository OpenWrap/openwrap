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
            var sourcePdbFile = sourceAssembly.Parent.GetFile(sourceAssembly.NameWithoutExtension + ".pdb");
            var sourcePdbStream = (Stream)null; //sourcePdbFile.Exists ? sourcePdbFile.OpenRead() : null;

            using(var sourceStream = sourceAssembly.OpenRead())
            {
                var readerParameters = new ReaderParameters();
                if (sourcePdbStream != null)
                {
                    readerParameters.SymbolStream = sourcePdbStream;
                    readerParameters.ReadSymbols = true;
                }
                var assembly = AssemblyDefinition.ReadAssembly(sourceStream, readerParameters);
                var name = assembly.Name;
                name.HashAlgorithm = AssemblyHashAlgorithm.SHA1;
                name.PublicKey = key.PublicKey;
                if (version != null)
                    name.Version = version;
                assembly.Name.HasPublicKey = true;
                var writerParameters = new WriterParameters() { StrongNameKeyPair = key };
                var destinationPdbStream = sourcePdbStream != null ? destinationAssembly.Parent.GetFile(destinationAssembly.NameWithoutExtension + ".pdb").OpenWrite() : null;
                if (sourcePdbStream != null)
                {
                    writerParameters.SymbolStream = destinationPdbStream;
                    writerParameters.WriteSymbols = true;
                }
                using (var destinationStream = destinationAssembly.Open(FileMode.Create, FileAccess.ReadWrite, FileShare.None))
                {
                    assembly.Write(destinationStream, writerParameters);
                }
                if (sourcePdbStream != null)
                {
                    sourcePdbStream.Close();
                    destinationPdbStream.Close();
                }
            }
        }
    }
}