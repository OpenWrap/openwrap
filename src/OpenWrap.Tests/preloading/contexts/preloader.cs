using System;
using System.Collections.Generic;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.Local;
using OpenWrap.IO;
using OpenWrap.IO.Packaging;
using OpenWrap.PackageModel;
using OpenWrap.Preloading;

namespace Tests.preloading.contexts
{
    abstract class preloader : IDisposable
    {
        IFileSystem file_system = LocalFileSystem.Instance;

        public preloader()
        {
                
        }
        protected void given_project_directory()
        {
            project_directory = file_system.CreateTempDirectory();
        }

        protected ITemporaryDirectory project_directory;
        protected IEnumerable<string> package_directories;
        protected Exception exception;

        public void Dispose()
        {
            project_directory.Dispose();
        }

        protected void when_locating_package(string packageName)
        {
            try
            {
                package_directories = Preloader.GetPackageFolders(Preloader.RemoteInstall.None, project_directory.Path, null, packageName);
            }
            catch(Exception e)
            {
                exception = e;
            }
        }

        protected void given_project_descriptor(string name, string descriptor = null)
        {
            project_directory.GetFile(name).WriteString(descriptor ?? string.Empty);
        }

        protected void given_project_package(string name, string version, params string[] descriptorLines)
        {
            Packager.NewWithDescriptor(
                project_directory.GetDirectory("wraps").GetFile(PackageNameUtility.PackageFileName(name, version)),
                name,
                version,
                descriptorLines);
        }
    }
}