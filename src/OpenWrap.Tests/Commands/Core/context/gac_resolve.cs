using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using NUnit.Framework;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.InMemory;
using OpenFileSystem.IO.FileSystems.Local;
using OpenWrap.Commands.Core;
using OpenWrap.IO.Packaging;
using OpenWrap.PackageManagement;
using OpenWrap.PackageManagement.Exporters;
using OpenWrap.PackageManagement.Exporters.Assemblies;
using OpenWrap.PackageManagement.Packages;
using OpenWrap.PackageModel;
using OpenWrap.Repositories;
using OpenWrap.Runtime;
using OpenWrap.Services;
using OpenWrap.Testing;

namespace OpenWrap.Tests.Dependencies
{
    public abstract class gac_resolve : OpenWrap.Testing.context
    {
        protected ILookup<IPackageInfo, AssemblyName> result;
        InMemoryRepository Repository = new InMemoryRepository("Memory");

        public gac_resolve()
        {
            FileSystem = LocalFileSystem.Instance;
            PackageFile = FileSystem.CreateTempFile();
            PackageFileDirectory = FileSystem.CreateTempDirectory();
            Exporter = new DefaultPackageExporter(new IExportProvider[]
            {
                    new DefaultAssemblyExporter()
            });
        }

        protected DefaultPackageExporter Exporter { get; set; }

        [TestFixtureTearDown]
        void DeleteTemp()
        {
            PackageFile.Dispose();
            PackageFileDirectory.Dispose();
        }
        protected ITemporaryFile PackageFile { get; set; }

        protected ITemporaryDirectory PackageFileDirectory { get; set; }

        protected IFileSystem FileSystem { get; set; }

        protected void when_loading_assembly_in_gac()
        {
            result = Exporter.InGac(new[]
            {
                    new CachedZipPackage(
                        Repository,
                        Packager.NewWithDescriptor(
                            PackageFile,
                            "package",
                            "1.0.0",
                            new[]
                            {
                                    new PackageContent
                                    {
                                            FileName = "System.Xml.dll",
                                            RelativePath = "bin-net20",
                                            Stream = () => File.OpenRead(typeof(XmlDocument).Assembly.Location)
                                    }
                            }),
                        PackageFileDirectory)
            });
        }
    }
}
