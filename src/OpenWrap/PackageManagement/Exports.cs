using System;
using System.Collections.Generic;
using System.Linq;
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
            AssemblyExportFlags Flags { get; }
            bool IsAssemblyReference { get; }
            bool IsRuntimeAssembly { get; }
        }
        public interface ICommand : IExportItem
        {
            ICommandDescriptor Descriptor { get; }
        }
        public interface IInstallHook : IExportItem
        {
            
        }
        public interface ISolutionPlugin : IExportItem
        {
            string Name { get; }
            IDisposable Start();
        }
    }
}