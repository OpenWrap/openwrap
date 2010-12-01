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
using OpenWrap.Dependencies;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace OpenWrap.Tests.Dependencies
{
    public abstract class gac_resolve : Testing.context
    {
        protected ILookup<IPackageInfo, AssemblyName> result;

        public gac_resolve()
        {
            FileSystem = LocalFileSystem.Instance;
            PackageFile = FileSystem.CreateTempFile();
            PackageFileDirectory = FileSystem.CreateTempDirectory();
        }
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
            result = GacResolver.InGac(new[]
            {
                    new CachedZipPackage(
                        null,
                        PackageBuilder.NewWithDescriptor(
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
                        PackageFileDirectory,
                        ExportBuilders.All)
            }, new ExecutionEnvironment{ Profile="net20", Platform="x86" });
        }
    }
}
