using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap;
using OpenWrap.PackageManagement.Exporters.Assemblies;
using OpenWrap.PackageModel;
using OpenWrap.Testing;

namespace Tests.Dependencies.parser
{
    public static class TestExtensions
    {
        public static VersionVertex AtLeast(this string version)
        {
            return new GreaterThanOrEqualVersionVertex(SemanticVersion.TryParseExact(version));
        }

        public static VersionVertex Exact(this string version)
        {
            return new EqualVersionVertex(SemanticVersion.TryParseExact(version));
        }

        public static VersionVertex ShouldAccept(this VersionVertex vertex, string version)
        {
            vertex.IsCompatibleWith(SemanticVersion.TryParseExact(version))
                .ShouldBeTrue();
            return vertex;
        }

        public static AbstractAssemblyExporter.EnvironmentDependentFile ShouldBeAfter(this AbstractAssemblyExporter.EnvironmentDependentFile file,
                                                                                      AbstractAssemblyExporter.EnvironmentDependentFile other)
        {
            var list = new List<AbstractAssemblyExporter.EnvironmentDependentFile>(new[] { file, other });
            list.Sort();
            list.Last().ShouldBe(file);
            return file;
        }

        public static AbstractAssemblyExporter.EnvironmentDependentFile ShouldBeBefore(this AbstractAssemblyExporter.EnvironmentDependentFile file,
                                                                                       AbstractAssemblyExporter.EnvironmentDependentFile other)
        {
            var list = new List<AbstractAssemblyExporter.EnvironmentDependentFile>(new[] { file, other });
            list.Sort();
            list.First().ShouldBe(file);
            return file;
        }

        public static VersionVertex ShouldNotAccept(this VersionVertex vertex, string version)
        {
            vertex.IsCompatibleWith(SemanticVersion.TryParseExact(version))
                .ShouldBeFalse();
            return vertex;
        }
    }
}