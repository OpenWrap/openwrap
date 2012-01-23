using System;
using System.Linq;
using NUnit.Framework;
using OpenWrap.PackageModel;
using OpenWrap.Testing;

namespace Tests.Repositories.indexed_http
{
    public class reading_the_dynamicproxy_package : context.wrap_list
    {
        IPackageInfo castle_proxy;

        public reading_the_dynamicproxy_package()
        {
            given_repository();
            castle_proxy = Repository.PackagesByName["castle-dynamicproxy"].First();

        }

        [Test]
        public void has_the_correct_name()
        {
            castle_proxy.Name.ShouldBe("castle-dynamicproxy");
        }
        [Test]public void has_the_correct_version()
        {
            castle_proxy.SemanticVersion.ShouldBe("2.1.0");
        }
        [Test]
        public void has_the_correct_dependencies()
        {
            castle_proxy.Dependencies.Count.ShouldBe(1);

            var core_dependency = castle_proxy.Dependencies.First();
            core_dependency.Name.ShouldBe("castle-core");
            core_dependency.ToString().ShouldBe("castle-core = 1.1.0");
            core_dependency.VersionVertices.First().Version.ShouldBe("1.1.0");
            core_dependency.VersionVertices.First().ShouldBeOfType<EqualVersionVertex>();
        }
    }
}