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
        }
        public interface ICommand : IExportItem
        {
            ICommandDescriptor Descriptor { get; }
        }

        public interface ISolutionPlugin : IExportItem
        {
            string Name { get; }
            IDisposable Start();
        }
        public static void main()
        {
            Console.WriteLine(new string[0].Any());
            Console.WriteLine(new string[0].Any(x => false));
            Console.WriteLine(new string[0].Any(x => true));
        }
    }
}