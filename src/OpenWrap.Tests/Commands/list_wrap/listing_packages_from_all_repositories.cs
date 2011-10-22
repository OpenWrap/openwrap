using System.Linq;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.Commands.Wrap;
using OpenWrap.PackageManagement;
using OpenWrap.Testing;

namespace Tests.Commands.list_wrap
{
    public class listing_packages_from_all_repositories : command<ListWrapCommand>
    {
        public listing_packages_from_all_repositories()
        {
            given_remote_repository("first");
            given_remote_repository("second");
            given_remote_package("first", "one-ring", "1.0.0".ToVersion());
            given_remote_package("second", "ring-of-power", "1.0.0".ToVersion());

            when_executing_command("ring -remote");
        }

        [Test]
        public void packages_are_found_in_any_remote()
        {
            Results.OfType<RepositoryPackages>()
                    .ShouldHaveCountOf(2)
                    .Check(_ => _.ElementAt(0).Packages.ShouldHaveOne(pack => pack.Name == "one-ring"))
                    .Check(_ => _.ElementAt(1).Packages.ShouldHaveOne(pack => pack.Name == "ring-of-power"));
        }
    }
}