using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.Commands.Wrap;
using OpenWrap.PackageManagement;
using OpenWrap.Testing;
using Tests.Commands.contexts;


namespace listWrap_specs
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
            Results.OfType<PackageFoundCommandOutput>()
                    .ShouldHaveCountOf(2)
                    .Check(x => x.ShouldHaveAtLeastOne(n => n.Name.Equals("one-ring")))
                    .Check(x => x.ShouldHaveAtLeastOne(n => n.Name.Equals("ring-of-power")));
        }
    }
}