using System.Collections.Generic;
using System.Reflection;
using OpenFileSystem.IO;
using OpenWrap.Commands;
using OpenWrap.PackageModel;

namespace OpenWrap.PackageManagement
{

    public static class Exports
    {
        public interface IFile : IExportItem
        {
            OpenFileSystem.IO.IFile File { get; }
        }
        public interface IAssembly : IFile
        {
            string Platform { get; }
            string Profile { get; }
            AssemblyName AssemblyName { get; }
        }
        public interface ICommand : IExportItem
        {
            ICommandDescriptor Descriptor { get; }
        }
    }
    public class Assembly : Exports.IAssembly{
        public Assembly(string path, IPackage package, IFile file, string platform, string profile, AssemblyName assemblyName)
        {
            Path = path;
            Package = package;
            File = file;
            Platform = platform;
            Profile = profile;
            AssemblyName = assemblyName;
        }

        public string Path { get; private set; }
        public IPackage Package { get; private set; }
        public IFile File { get; private set; }
        public string Platform { get; private set; }
        public string Profile { get; private set; }
        public AssemblyName AssemblyName { get; private set; }
    }
}