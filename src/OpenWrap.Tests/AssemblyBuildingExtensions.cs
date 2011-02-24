using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using OpenFileSystem.IO;
using Path = System.IO.Path;

namespace OpenWrap
{
    public static class AssemblyBuildingExtensions
    {
        public static Stream CreateEmptyAssembly(string assemblyName)
        {
            var tempAssembly = Path.GetTempFileName();
            try
            {
                var asmName = new AssemblyName(assemblyName);
                var asmBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.Save | AssemblyBuilderAccess.ReflectionOnly, Path.GetDirectoryName(tempAssembly));

                var mb = asmBuilder.DefineDynamicModule(assemblyName + ".dll");

                asmBuilder.Save(Path.GetFileName(tempAssembly));
                return new MemoryStream(File.ReadAllBytes(tempAssembly));
            }
            finally
            {
                try
                {
                    if (File.Exists(tempAssembly))
                        File.Delete(tempAssembly);
                }
                catch (IOException) { }
            }
        }
        public static IFile CreateEmptyAssembly(this IDirectory directory, string assemblyName)
        {
            var assemblyFile = directory.GetFile(assemblyName + ".dll");
            using(var assemblyStream = assemblyFile.OpenWrite())
                CreateEmptyAssembly(assemblyName).CopyTo(assemblyStream);
            return assemblyFile;
        }
    }
}
