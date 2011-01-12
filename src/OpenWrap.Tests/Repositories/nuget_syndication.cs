using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Xml;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.Repositories;
using OpenWrap.Repositories.Http;
using OpenWrap.Repositories.NuGet;
using OpenWrap.Testing;
using OpenWrap.Tests.Repositories;

namespace nuget_syndication_specs
{
    public class parsing_odata_syndication_feed : parsing_syndication_feed
    {
        protected override string GetFeedContent()
        {
            return TestFiles.feedodata;
        }
    }
    public class parsing_syndication_feed : context.nuget_syndication
    {
        public parsing_syndication_feed()
        {
            given_syndication_feed(GetFeedContent());

            when_parsing_package_document();
        }

        protected virtual string GetFeedContent()
        {
            return TestFiles.feed;
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
                    .Check(x => x.CreationTime.ShouldBe(DateTimeOffset.Parse("2010-10-11T07:08:57Z").ToUniversalTime()))
                    .Check(x => x.Version.ShouldBe("2.2.4.900".ToVersion()));
        }
        [Test]
        public void package_has_correct_download_link()
        {
            var autofacPackage = PackageDoc.Packages.FirstOrDefault(x => x.Name == "Autofac");
            autofacPackage.PackageHref.ShouldBe("http://173.203.67.148/file.nupkg");
        }
        [Test]
        public void package_has_correct_dependencies()
        {
            var autofacMvcPackage = PackageDoc.Packages.FirstOrDefault(x => x.Name == "Autofac.MVC2");

            autofacMvcPackage.Dependencies
                    .ShouldHaveCountOf(1)
                    .First().ShouldBe("Autofac = 2.2.4.900");
        }
    }
    namespace context
    {
        public abstract class nuget_syndication : OpenWrap.Testing.context
        {
            protected NuGetSyndicationFeed Feed;
            protected PackageDocument PackageDoc;

            protected void when_parsing_package_document()
            {
                PackageDoc = Feed.ToPackageDocument();
            }

            protected void given_syndication_feed(string feedContent)
            {
                Feed = SyndicationFeed.Load<NuGetSyndicationFeed>(new XmlTextReader(new StringReader(feedContent)));
            }
        }
    }
}
