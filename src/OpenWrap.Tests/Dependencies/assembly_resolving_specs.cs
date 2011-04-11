using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using NUnit.Framework;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.InMemory;
using OpenFileSystem.IO.FileSystems.Local;
using OpenWrap;
using OpenWrap.Commands.contexts;
using OpenWrap.IO.Packaging;
using OpenWrap.PackageManagement;
using OpenWrap.PackageManagement.DependencyResolvers;
using OpenWrap.PackageManagement.Exporters;
using OpenWrap.Repositories;
using OpenWrap.Testing;

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
    public class recursive_dependencies_for_assemblies : contexts.assembly_resolving
    {
        public recursive_dependencies_for_assemblies()
        {
            given_dependency("depends: openwrap content");
            given_dependency("depends: openfilesystem");
            given_dependency("depends: sharpziplib");
            given_project_package("openwrap", "1.0.0.0", Assemblies(Assembly("openwrap", "bin-net35")), "depends: openwrap content", "depends: sharpziplib", "depends: openfilesystem");
            given_project_package("sharpziplib", "1.0.0.0", Assemblies(Assembly("sharpziplib", "bin-net35")));
            given_project_package("openfilesystem", "1.0.0.0", Assemblies(Assembly("openfilesystem", "bin-net35")), "depends: openwrap content", "depends: sharpziplib");

            when_resolving_assemblies("anyCPU", "net35");
        }
        [Test]
        public void assemblies()
        {
            AssemblyReferences.ShouldHaveCountOf(2);

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
    [TestFixture("eastbight")]
    [TestFixture("eastbight.dll")]
    public class assemblies_defined_in_reference_section_are_resolved_by_assembly_name : contexts.assembly_resolving
    {
        public assemblies_defined_in_reference_section_are_resolved_by_assembly_name(string assemblyName)
        {
            given_dependency("depends: east-bight");
            given_project_package("east-bight", "1.0.0.0", Assemblies(Assembly("eastbight", "bin-net35"), Assembly("eastbight.runner", "bin-net35")), "referenced-assemblies: " + assemblyName);

            when_resolving_assemblies("anyCPU", "net35");
        }

        [Test]
        public void included_assembly_is_referenced()
        {
            AssemblyReferences.ShouldHaveCountOf(1)
                    .First().AssemblyName.Name.ShouldBe("eastbight");
        }

        [Test]
        public void excluded_assembly_is_not_resolved()
        {
            AssemblyReferences.Any(x => x.AssemblyName.Name == "eastbight.runer")
                    .ShouldBeFalse();
        }
    }
    public class multiple_assemblies_defined_in_reference_section_are_resolved_by_assembly_name : contexts.assembly_resolving
    {
        public multiple_assemblies_defined_in_reference_section_are_resolved_by_assembly_name()
        {
            given_dependency("depends: east-bight");
            given_project_package("east-bight", "1.0.0.0",
                Assemblies(Assembly("eastbight", "bin-net35"), Assembly("eastbight.runner", "bin-net35")),
                "referenced-assemblies: eastbight, eastbight.runner");

            when_resolving_assemblies("anyCPU", "net35");
        }

        [Test]
        public void included_assembly_is_referenced()
        {
            AssemblyReferences.ShouldHaveCountOf(2)
                    .Check(x=>x.Any(asm=>asm.AssemblyName.Name == "eastbight").ShouldBeTrue())
                    .Check(x=>x.Any(asm=>asm.AssemblyName.Name == "eastbight.runner").ShouldBeTrue());
        }
    }
    namespace contexts
    {
        public abstract class assembly_resolving : command
        {
            protected IEnumerable<Exports.IAssembly> AssemblyReferences;
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
            protected void given_project_package(string name, string version, IEnumerable<PackageContent> content, params string[] descriptorLines)
            {
                
                var packageFile = Package(name, version, content, descriptorLines);
                var packageStream = packageFile.OpenRead();
                using (var publisher = ((ISupportPublishing)Environment.ProjectRepository).Publisher())
                using (var readStream = packageStream)
                    publisher.Publish(packageFile.Name, readStream);
            }

            protected IEnumerable<PackageContent> Assemblies(params PackageContent[] assemblies)
            {
                return assemblies;
            }
            protected PackageContent Assembly(string assemblyName, string relativePath)
            {
                var assemblyFile = TempDirectory.CreateEmptyAssembly(assemblyName);
                return new PackageContent { FileName = assemblyFile.Name, RelativePath = relativePath, Stream = () => assemblyFile.OpenRead(), Size = assemblyFile.Size };

            }
            protected InMemoryFile Package(string name, string version, IEnumerable<PackageContent> content, params string[] descriptorLines)
            {
                var file = new InMemoryFile(name + "-" + version + ".wrap");
                Packager.NewWithDescriptor(file, name, version, content, descriptorLines);
                return file;
            }

            protected void when_resolving_assemblies(string platform, string profile)
            {
                throw new NotImplementedException();
                //AssemblyReferences = PackageResolver.GetAssemblyReferences(false, Environment.Descriptor, Environment.ProjectRepository);
            }
            [TestFixtureTearDown]
            public void Dispose()
            {
                TempDirectory.Delete();
            }
        }
    }
}
