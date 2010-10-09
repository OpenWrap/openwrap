using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Xml;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.Repositories.Http;
using OpenWrap.Repositories.NuPack;
using OpenWrap.Testing;
using OpenWrap.Tests.Repositories;

namespace nupack_syncidcation_specs
{
    public class parsing_syndication_feed : context.nupack_syndication
    {
        public parsing_syndication_feed()
        {
            given_syndication_feed(TestFiles.feed);

            when_parsing_package_document();
        }
        [Test]
        public void two_packages_are_present()
        {
            PackageDoc.Packages.ShouldHaveCountOf(2);
        }
        [Test]
        public void package_has_correct_metadata()
        {
            var autofacPackage = PackageDoc.Packages.FirstOrDefault(x => x.Name == "Autofac");

            autofacPackage.ShouldNotBeNull()
                    .Check(x => x.LastModifiedTimeUtc.ShouldBe(DateTime.Parse("2010-10-09T08:38:17Z").ToUniversalTime()))
                    .Check(x => x.Version.ShouldBe("2.2.4.900".ToVersion()));
        }
        [Test]
        public void pacakge_has_correct_dependencies()
        {
            var autofacMvcPackage = PackageDoc.Packages.FirstOrDefault(x => x.Name == "Autofac.MVC2");

            autofacMvcPackage.Dependencies
                    .ShouldHaveCountOf(1)
                    .First().ShouldBe("depends: Autofac = 2.2.4.900");
        }
    }
    namespace context
    {
        public abstract class nupack_syndication : OpenWrap.Testing.context
        {
            protected NuPackSyndicationFeed Feed;
            protected PackageDocument PackageDoc;

            protected void when_parsing_package_document()
            {
                PackageDoc = Feed.ToPackageDocument();
            }

            protected void given_syndication_feed(string feedContent)
            {
                Feed = SyndicationFeed.Load<NuPackSyndicationFeed>(new XmlTextReader(new StringReader(feedContent)));
            }
        }
    }
}
