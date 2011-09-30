using System;
using System.Linq;
using NUnit.Framework;
using OpenWrap.Build.PackageBuilders;
using OpenWrap.Testing;

namespace Tests.Build.builders.msbuild
{
    public class project_doesnt_exist : contexts.msbuild_builder
    {
        public project_doesnt_exist()
        {
            given_empty_directory();
            when_building("test1.csproj");
        }
        [Test]
        public void build_fails()
        {
            Results.OfType<UnknownProjectFileResult>().ShouldNotBeEmpty()
                .First().Spec.ShouldBe("test1.csproj");
        }
    }
}