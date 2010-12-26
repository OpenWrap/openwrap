using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenWrap.PackageManagement.Packages;
using OpenWrap.PackageModel;
using OpenWrap.Repositories;

namespace OpenWrap.Tests.Dependencies
{
    public class when_resolving_build_dependency_against_nuked_revision : ctxt.nuked_package_resolution
    {
        public when_resolving_build_dependency_against_nuked_revision()
        {
            given_package_version("1.0.0.0");
            given_nuked_package_version("1.0.0.1");
            given_dependency(new EqualVersionVertex(new Version("1.0.0")));
            when_resolving();
        }

        [Test]
        public void the_non_nuked_revision_is_returned()
        {
            ResolvedVersion.Equals("1.0.0.0");
        }
    }

    public class when_resolving_minor_dependency_against_nuked_build : ctxt.nuked_package_resolution
    {
        public when_resolving_minor_dependency_against_nuked_build()
        {
            given_package_version("2.1.0.0");
            given_nuked_package_version("2.1.1.0");
            given_dependency(new EqualVersionVertex(new Version("2.1")));
            when_resolving();
        }

        [Test]
        public void the_non_nuked_revision_is_returned()
        {
            ResolvedVersion.Equals("2.1.0.0");
        }
    }

    public class when_resolving_build_dependency_against_nuked_build : ctxt.nuked_package_resolution
    {
        public when_resolving_build_dependency_against_nuked_build()
        {
            given_package_version("2.1.0");
            given_nuked_package_version("2.1.1");
            given_dependency(new EqualVersionVertex(new Version("2.1.1")));
            when_resolving();
        }

        [Test]
        public void the_nuked_revision_is_returned()
        {
            ResolvedVersion.Equals("2.1.1");
        }
    }

    namespace ctxt
    {
        public class nuked_package_resolution : OpenWrap.Testing.context
        {
            protected string ResolvedVersion;
            private PackageDependencyBuilder _packageDependency
                = new PackageDependencyBuilder(packageName);
            private List<IPackageInfo> _versions = new List<IPackageInfo>(); 

            const string packageName="test";

            protected void given_package_version(string version)
            {
                _versions.Add(new InMemoryPackage()
                {
                    Name = packageName,
                    Version = new Version(version)
                });
            }

            protected void given_nuked_package_version(string version)
            {
                _versions.Add(new InMemoryPackage()
                {
                    Name = packageName,
                    Version = new Version(version),
                    Nuked = true
                });
            }

            protected void given_dependency(VersionVertex vertex)
            {
                _packageDependency.VersionVertex(vertex);
            }

            protected void when_resolving()
            {
                ResolvedVersion = _versions.ToLookup(x=>x.Name)
                    .Find(_packageDependency)
                    .Version.ToString();
            }
        }
    }
}
