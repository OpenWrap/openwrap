using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using NUnit.Framework;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystem.InMemory;
using OpenFileSystem.IO.FileSystem.Local;
using OpenWrap.Dependencies;
using OpenWrap.Exports;
using OpenWrap.Repositories;
using OpenWrap.Testing;
using OpenWrap.Tests.Commands.context;

namespace assembly_resolving_specs
{
    public class resolving_assemblies_when_invalid_dependencies : contexts.assembly_resolving
    {
        public resolving_assemblies_when_invalid_dependencies()
        {
            given_dependency("depends: mirkwood");
            given_dependency("depends: eastbight");
            given_project_package("mirkwood", "1.0.0.0", Assemblies(Assembly("mirkwood", "bin-net35")));

            when_resolving_assemblies("anyCPU", "net35");
        }
        [Test]
        public void assemblies_from_valid_packages_are_loaded()

        {
            AssemblyReferences.ShouldHaveCountOf(1).First().AssemblyName.Name.ShouldBe("mirkwood");
        }
    }
    public class marking_dependency_as_content : contexts.assembly_resolving
    {
        public marking_dependency_as_content()
        {
            given_dependency("depends: mirkwood content");
            given_project_package("mirkwood", "1.0.0.0", Assemblies(Assembly("mirkwood", "bin-net35")));

            when_resolving_assemblies("anyCPU", "net35");
        }
        [Test]
        public void assemblies_are_ignored()
        {
            AssemblyReferences.ShouldBeEmpty();
        }
    }
    public class marking_dependency_with_sub_dependencies_as_content : contexts.assembly_resolving
    {
        public marking_dependency_with_sub_dependencies_as_content()
        {
            given_dependency("depends: mirkwood content");
            given_project_package("east-bight", "1.0.0.0", Assemblies(Assembly("esatbight", "bin-net35")));
            given_project_package("mirkwood", "1.0.0.0", Assemblies(Assembly("mirkwood", "bin-net35")), "depends: east-bight");

            when_resolving_assemblies("anyCPU", "net35");
        }
        [Test]
        public void assemblies_are_ignored()
        {
            AssemblyReferences.ShouldBeEmpty();
        }
    }
    public class assemblies_are_found : contexts.assembly_resolving
    {
        public assemblies_are_found()
        {
            given_dependency("depends: mirkwood");
            given_project_package("east-bight", "1.0.0.0", Assemblies(Assembly("eastbight", "bin-net35")));
            given_project_package("mirkwood", "1.0.0.0", Assemblies(Assembly("mirkwood", "bin-net35")), "depends: east-bight");

            when_resolving_assemblies("anyCPU", "net35");
        }
        [Test]
        public void assemblies_are_ignored()
        {
            AssemblyReferences.ShouldHaveCountOf(2)
                    .Check(x => x.FirstOrDefault(y => y.AssemblyName.Name == "eastbight").ShouldNotBeNull())
                    .Check(x => x.FirstOrDefault(y => y.AssemblyName.Name == "mirkwood").ShouldNotBeNull());
        }
    }
    namespace contexts
    {
        public abstract class assembly_resolving : openwrap_context
        {
            protected IEnumerable<IAssemblyReferenceExportItem> AssemblyReferences;
            ITemporaryDirectory TempDirectory;

            public assembly_resolving()
            {

                given_project_repository(new FolderRepository(FileSystem.GetTempDirectory().GetDirectory(Guid.NewGuid().ToString()).MustExist()));
                this.PackageResolver = new ExhaustiveResolver();
            }


            protected ExhaustiveResolver PackageResolver { get; set; }

            protected override IFileSystem given_file_system(string currentDirectory)
            {
                TempDirectory = LocalFileSystem.Instance.CreateTempDirectory();
                return LocalFileSystem.Instance;
            }
            protected void given_project_package(string name, string version, IEnumerable<PackageContent> content, params string[] dependencies)
            {
                var packageFileName = name + "-" + version + ".wrap";
                var packageStream = PackageBuilder.NewWithDescriptor(new InMemoryFile(packageFileName), name, version, content, dependencies).OpenRead();
                using (var readStream = packageStream)
                    ((ISupportPublishing)Environment.ProjectRepository).Publish(packageFileName, readStream);
            }

            protected IEnumerable<PackageContent> Assemblies(params PackageContent[] assemblies)
            {
                return assemblies;
            }
            protected PackageContent Assembly(string assemblyName, string content)
            {
                var assemblyFile = TempDirectory.GetFile(assemblyName + ".dll");
                var asmName = new AssemblyName(assemblyName);
                var asmBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.Save | AssemblyBuilderAccess.ReflectionOnly, assemblyFile.Parent.Path.FullPath);
                var mb = asmBuilder.DefineDynamicModule(assemblyName + ".dll");
                asmBuilder.Save(assemblyFile.Name);

                return new PackageContent { FileName = assemblyFile.Name, RelativePath = content, Stream = () => assemblyFile.OpenRead(), Size = new System.IO.FileInfo(assemblyFile.Path.FullPath).Length };

            }
            protected InMemoryFile Package(string wrapName, string version, IEnumerable<PackageContent> content, params string[] wrapdescLines)
            {
                var file = new InMemoryFile(wrapName + "-" + version + ".wrap");
                PackageBuilder.NewWithDescriptor(file, wrapName, version, content, wrapdescLines);
                return file;
            }

            protected void when_resolving_assemblies(string platform, string profile)
            {
                AssemblyReferences = PackageResolver.GetAssemblyReferences(false, new ExecutionEnvironment(platform, profile), Environment.Descriptor, Environment.ProjectRepository);
            }
            [TestFixtureTearDown]
            public void Dispose()
            {
                TempDirectory.Delete();
            }
        }
    }
}
