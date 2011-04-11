using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using OpenWrap.PackageManagement;
using OpenWrap.PackageManagement.Exporters;
using OpenWrap.PackageModel;
using OpenWrap.PackageModel.Parsers;
using OpenWrap.Repositories;
using OpenWrap.Repositories.Wrap.Tests.Dependencies.contexts;
using OpenWrap.Testing;

namespace OpenWrap.Repositories.Wrap.Tests.Dependencies
{
    public class when_parsing_declaration_with_multiple_versions : dependency_parser_context
    {
        public when_parsing_declaration_with_multiple_versions()
        {
            given_dependency("depends: nhibernate >= 2.1 and < 3.0");
        }

        [Test]
        public void the_versions_are_processed()
        {
            Declaration.VersionVertices.Count().ShouldBe(2);

            Declaration.IsFulfilledBy(new Version("2.1.0.0")).ShouldBeTrue();
            Declaration.IsFulfilledBy(new Version("3.0.0.0")).ShouldBeFalse();
        }
    }
    public class when_parsing_anchored_dependency_without_version : dependency_parser_context
    {
        public when_parsing_anchored_dependency_without_version()
        {
            given_dependency("depends: nhibernate anchored");
        }
        [Test]
        public void the_anchor_is_found()
        {
            Declaration.Name.ShouldBe("nhibernate");
            Declaration.Anchored.ShouldBeTrue();
            
        }
    }
    public class when_package_name_is_anchored : dependency_parser_context
    {
        public when_package_name_is_anchored()
        {
            given_dependency("depends: anchored anchored");
        }
        [Test]
        public void the_anchor_is_found()
        {
            Declaration.Name.ShouldBe("anchored");
            Declaration.Anchored.ShouldBeTrue();

        }
    }
    public class when_parsing_anchored_dependency_with_version : dependency_parser_context
    {
        public when_parsing_anchored_dependency_with_version()
        {
            given_dependency("depends: nhibernate = 2.0 anchored");
        }
        [Test]
        public void the_anchor_is_found()
        {
            Declaration.Name.ShouldBe("nhibernate");
            Declaration.Anchored.ShouldBeTrue();
            Declaration.IsFulfilledBy(new Version("2.0.0.0"));
        }
    }
    public class when_parsing_declaration_without_version : dependency_parser_context
    {
        public when_parsing_declaration_without_version()
        {
            given_dependency("depends: nhibernate");
        }

        [Test]
        public void the_name_is_parsed()
        {
            Declaration.Name.ShouldBe("nhibernate");
        }

        [Test]
        public void the_version_is_any()
        {
            Declaration.VersionVertices.First().ShouldBeOfType<AnyVersionVertex>()
                .ShouldAccept("0.0.0.0");
        }
    }

    public class when_parsing_simple_declaration_with_version : dependency_parser_context
    {
        public when_parsing_simple_declaration_with_version()
        {
            given_dependency("depends: nhibernate >= 2.1");
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
                .First().ShouldBeOfType<GreaterThanOrEqualVersionVertex>()
                .ShouldAccept("2.1.0.0")
                .ShouldAccept("3.0");
        }
    }

    namespace contexts
    {
        public abstract class dependency_parser_context : Testing.context
        {
            protected PackageDependency Declaration { get; set; }

            public void given_dependency(string dependencyLine)
            {
                var target = new PackageDescriptor();
                new DependsParser().Parse(dependencyLine, target);
                Declaration = target.Dependencies.First();
            }
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

    public class parsing_wrap_names : Testing.context
    {
        public void version_is_parsed()
        {
            PackageNameUtility.GetVersion("openrasta-2.0.0").ShouldBe(new Version("2.0.0"));
            PackageNameUtility.GetName("openrasta-2.0.0").ShouldBe("openrasta");

        }

        public void invalid_version_is_ignored()
        {
            PackageNameUtility.GetVersion("openrasta-2.0").ShouldBeNull();
            PackageNameUtility.GetName("openrasta-2.0").ShouldBe("openrasta-2.0");
        }
        
    }

    public class InMemItem : IExportItem
    {
        public string Path { get; set; }

        public IPackage Package { get; set; }
    }

    public static class TestExtensions
    {
        public static VersionVertex AtLeast(this string version)
        {
            return new GreaterThanOrEqualVersionVertex(new Version(version));
        }

        public static VersionVertex Exact(this string version)
        {
            return new EqualVersionVertex(new Version(version));
        }

        public static VersionVertex ShouldAccept(this VersionVertex vertex, string version)
        {
            vertex.IsCompatibleWith(new Version(version))
                .ShouldBeTrue();
            return vertex;
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

        public static VersionVertex ShouldNotAccept(this VersionVertex vertex, string version)
        {
            vertex.IsCompatibleWith(new Version(version))
                .ShouldBeFalse();
            return vertex;
        }
    }
}