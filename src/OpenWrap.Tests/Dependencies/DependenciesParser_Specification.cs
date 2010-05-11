using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using OpenRasta.Testing;
using OpenRasta.Wrap.Dependencies;
using OpenRasta.Wrap.Repositories;
using OpenRasta.Wrap.Resources;
using OpenRasta.Wrap.Tests.Dependencies.contexts;

namespace OpenRasta.Wrap.Tests.Dependencies
{
    public class when_parsing_declaration_with_multiple_versions : dependency_parser_context
    {
        public when_parsing_declaration_with_multiple_versions()
        {
            given_dependency("depends nhibernate >= 2.1 and < 3.0");
        }

        [Test]
        public void the_versions_are_processed()
        {
            Declaration.VersionVertices.Count.ShouldBe(2);

            Declaration.IsFulfilledBy(new Version("2.1.0.0")).ShouldBeTrue();
            Declaration.IsFulfilledBy(new Version("3.0.0.0")).ShouldBeFalse();
        }
    }

    public class when_parsing_declaration_without_version : dependency_parser_context
    {
        public when_parsing_declaration_without_version()
        {
            given_dependency("depends nhibernate");
        }

        [Test]
        public void the_name_is_parsed()
        {
            Declaration.Name.ShouldBe("nhibernate");
        }

        [Test]
        public void the_version_is_any()
        {
            Declaration.VersionVertices.First().ShouldBeOfType<AnyVersionVertice>()
                .ShouldAccept("0.0.0.0");
        }
    }

    public class when_parsing_simple_declaration_with_version : dependency_parser_context
    {
        public when_parsing_simple_declaration_with_version()
        {
            given_dependency("depends nhibernate >= 2.1");
        }

        [Test]
        public void the_name_is_parsed()
        {
            Declaration.Name.ShouldBe("nhibernate");
        }

        [Test]
        public void the_version_vertice_is_of_correct_type()
        {
            Declaration.VersionVertices
                .First().ShouldBeOfType<AtLeastVersionVertice>()
                .ShouldAccept("2.1.0.0")
                .ShouldAccept("3.0");
        }
    }

    namespace contexts
    {
        public abstract class dependency_parser_context : context
        {
            protected WrapDependency Declaration { get; set; }

            public void given_dependency(string dependencyLine)
            {
                var target = new WrapDescriptor();
                new WrapDependencyParser().Parse(dependencyLine, target);
                Declaration = target.Dependencies.First();
            }
        }
    }

    public class when_versions_are_matched_exactly : context
    {
        [Test]
        public void a_higher_build_is_compatible()
        {
            "1.0.0.0".Exact().ShouldAccept("1.0.1.0");
        }

        [Test]
        public void a_lower_build_is_not_compatible()
        {
            "1.0.1.0".Exact().ShouldNotAccept("1.0.0.0");
        }

        [Test]
        public void the_exact_version_is_compatble()
        {
            "1.0.0.0".Exact().ShouldAccept("1.0.0.0");
        }

        [Test]
        public void the_revision_is_ignored()
        {
            "1.0.0.0".Exact().ShouldAccept("1.0.0.1");
        }
    }

    public class when_version_is_at_least : context
    {
        [Test]
        public void higer_minor_is_compatible()
        {
            "2.1.2.3".AtLeast()
                .ShouldAccept("2.2.0.0");
        }

        [Test]
        public void higher_build_is_compatible()
        {
            "2.1.2.3".AtLeast().ShouldAccept("2.2.0.0");
        }

        [Test]
        public void higher_major_is_compatible()
        {
            "2.1.2.3".AtLeast()
                .ShouldAccept("3.0.0.0");
        }

        [Test]
        public void revision_is_ignored()
        {
            "2.1.2.3".AtLeast().ShouldAccept("2.1.2.0");
        }

        [Test]
        public void version_above_is_compatible()
        {
            "2.1".AtLeast().ShouldAccept("2.1.0.0");
        }

        [Test]
        public void versions_under_are_not_compatible()
        {
            "2.1.2.3".AtLeast()
                .ShouldNotAccept("2.1.0.0")
                .ShouldNotAccept("2.0.0.0")
                .ShouldNotAccept("1.0.0.0");
        }
    }

    public class Sorting_of_environment_dependent_files
    {
        [Test]
        public void elements_are_sorted_by_target_first()
        {
            File("AnyCPU", "net35").ShouldBeBefore(File("x86", "net30"));
        }

        [Test]
        public void elements_with_same_target_are_sorted_for_platform_first()
        {
            File("AnyCPU", "net30").ShouldBeAfter(File("x86", "net30"));
        }

        public void elements_with_specific_architectures_win()
        {
            File("x86", "net35").ShouldBeBefore(File("x86", "net20"));
        }

        EnvironmentDependentFile File(string platform, string target)
        {
            return new EnvironmentDependentFile
            {
                Platform = platform,
                Profile = target
            };
        }
    }

    public class AssemblyReferenceExportBuilder_context : context
    {
        IWrapExport[] _exports;
        string _result;

        [Test]
        public void closest_architecture_is_found()
        {
            given(
                Export("bin-net35", @"Sauron.dll"),
                Export("bin-x86-net35", @"Sauron.dll"));

            when_exporting_for("x86", "net35");

            _result.ShouldBe(@"c:\bin-x86-net35\Sauron.dll");
        }

        [Test]
        public void downlevel_target_is_found()
        {
            given(
                Export("bin-net35", @"Sauron.dll"),
                Export("bin-net20", @"Sauron.dll"));

            when_exporting_for("AnyCPU", "net30");

            _result.ShouldBe(@"c:\bin-net20\Sauron.dll");
        }

        public IWrapExport Export(string folder, string assemblyName)
        {
            return new InMemExport
            {
                Name = folder,
                Items = new[]
                {
                    new InMemItem { FullPath = "c:\\" + folder + "\\" + assemblyName }
                }
            };
        }

        public void given(params IWrapExport[] exports)
        {
            _exports = exports;
        }

        [Test]
        public void highest_target_is_found()
        {
            given(
                Export("bin-net35", @"Sauron.dll"),
                Export("bin-net20", @"Sauron.dll"));

            when_exporting_for("AnyCPU", "net35");

            _result.ShouldBe(@"c:\bin-net35\Sauron.dll");
        }

        [Test]
        public void incompatbile_architectures_are_ignored_for_anycpu_environment()
        {
            given(
                Export("bin-x86net35", @"Sauron.dll"),
                Export("bin-x64-net35", @"Sauron.dll"));

            when_exporting_for("x64", "net35");

            _result.ShouldBe(@"c:\bin-x64-net35\Sauron.dll");
        }

        [Test]
        public void specific_architectures_are_ignored_for_anycpu_environment()
        {
            given(
                Export("bin-net35", @"Sauron.dll"),
                Export("bin-x86-net35", @"Sauron.dll"));

            when_exporting_for("AnyCPU", "net35");

            _result.ShouldBe(@"c:\bin-net35\Sauron.dll");
        }

        public void when_exporting_for(string platform, string targetProfile)
        {
            IWrapExport processExports = new AssemblyReferenceExportBuider().ProcessExports(_exports,
                                                                                            new WrapRuntimeEnvironment
                                                                                            {
                                                                                                Platform = platform,
                                                                                                Profile =
                                                                                                    targetProfile
                                                                                            });
            _result = processExports.Items
                .Single(x => Path.GetFileName(x.FullPath) == "Sauron.dll").FullPath;
        }
    }

    public class parsing_wrap_names : context
    {
        public void version_is_parsed()
        {
            WrapNameUtility.GetVersion("openrasta-2.0.0").ShouldBe(new Version("2.0.0"));
            WrapNameUtility.GetName("openrasta-2.0.0").ShouldBe("openrasta");

        }

        public void invalid_version_is_ignored()
        {
            WrapNameUtility.GetVersion("openrasta-2.0").ShouldBeNull();
            WrapNameUtility.GetName("openrasta-2.0").ShouldBe("openrasta-2.0");
        }
        
    }

    public class InMemItem : IWrapExportItem
    {
        public string FullPath { get; set; }
    }

    public class InMemExport : IWrapExport
    {
        public InMemExport()
        {
            Items = new List<IWrapExportItem>();
        }

        public IEnumerable<IWrapExportItem> Items { get; set; }
        public string Name { get; set; }
    }

    public static class TestExtensions
    {
        public static VersionVertice AtLeast(this string version)
        {
            return new AtLeastVersionVertice(new Version(version));
        }

        public static VersionVertice Exact(this string version)
        {
            return new ExactVersionVertice(new Version(version));
        }

        public static VersionVertice ShouldAccept(this VersionVertice vertice, string version)
        {
            vertice.IsCompatibleWith(new Version(version))
                .ShouldBeTrue();
            return vertice;
        }

        public static EnvironmentDependentFile ShouldBeAfter(this EnvironmentDependentFile file,
                                                             EnvironmentDependentFile other)
        {
            var list = new List<EnvironmentDependentFile>(new[] { file, other });
            list.Sort();
            list.Last().ShouldBe(file);
            return file;
        }

        public static EnvironmentDependentFile ShouldBeBefore(this EnvironmentDependentFile file,
                                                              EnvironmentDependentFile other)
        {
            var list = new List<EnvironmentDependentFile>(new[] { file, other });
            list.Sort();
            list.First().ShouldBe(file);
            return file;
        }

        public static VersionVertice ShouldNotAccept(this VersionVertice vertice, string version)
        {
            vertice.IsCompatibleWith(new Version(version))
                .ShouldBeFalse();
            return vertice;
        }
    }
}