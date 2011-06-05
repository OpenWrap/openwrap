using System.Linq;
using NUnit.Framework;
using OpenWrap.Repositories;
using Tests.Repositories.contexts;

namespace Tests.Repositories.manager
{
    class repositories_by_configured_name_and_additional_factory : remote_manager
    {
        public repositories_by_configured_name_and_additional_factory()
        {
            given_remote_repository("iron-hills", priority: 10);
            given_remote_factory_additional(userInput => new InMemoryRepository("celduin"));
            when_listing_repositories("iron-hills");
        }

        [Test]
        public void configured_is_returned()
        {
            Enumerable.Single<IPackageRepository>(FetchRepositories.ShouldHaveCountOf(1)).Name.ShouldBe("iron-hills");
        }
    }
}