using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenFileSystem.IO.FileSystems.Local;
using OpenRasta.Client;
using OpenWrap;
using OpenWrap.Repositories.NuFeed;
using OpenWrap.Testing;
using Tests;

namespace Tests.Repositories.nufeed
{
    [TestFixture, Explicit]
    public class reading_official_nuget_feed2
    {
        [Test]
        public void packages()
        {
            var factory = new NuFeedRepositoryFactory(LocalFileSystem.Instance, new WebRequestHttpClient());
            factory.FromUserInput("nuget").PackagesByName.Count().ShouldBe(22);
        }
    }
}