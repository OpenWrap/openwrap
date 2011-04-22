using System.Collections.Generic;
using System.Reflection;
using OpenWrap.Commands;

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
}