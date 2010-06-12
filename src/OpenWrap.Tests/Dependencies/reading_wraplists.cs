using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using NUnit.Framework;
using OpenWrap.Exports;
using OpenWrap.IO;
using OpenWrap.Repositories;
using OpenWrap.Dependencies;
using OpenWrap.Testing;

namespace OpenWrap.Repositories.Wrap.Tests.Dependencies
{
    public class reading_a_wrap_list : context.wrap_list
    {
        public reading_a_wrap_list()
        {
            given_repository();
        }

        [Test]
        public void there_are_4_packages()
        {
            Repository.PackagesByName.Count().ShouldBe(4);
        }
    }
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
            castle_proxy.Version.ShouldBe(new Version(2, 1, 0));
        }
        [Test]
        public void has_the_correct_dependencies()
        {
            castle_proxy.Dependencies.Count.ShouldBe(1);

            var core_dependency = castle_proxy.Dependencies.First();
            core_dependency.Name.ShouldBe("castle-core");
            core_dependency.ToString().ShouldBe("castle-core = 1.1.0");
            core_dependency.VersionVertices.First().Version.ShouldBe(new Version("1.1.0"));
            core_dependency.VersionVertices.First().ShouldBeOfType<ExactVersionVertice>();
        }
    }
    namespace context
    {
        public class wrap_list : Testing.context
        {
            protected void given_repository()
            {
                Repository = new XmlRepository(FileSystem.Local, new InMemoryNavigator(), new IExportBuilder[0]);
            }
            class InMemoryNavigator : IHttpNavigator
            {
                public XDocument LoadFileList()
                {
                    
                    var doc =XDocument.Parse(WrapListDocument, LoadOptions.SetBaseUri);
                    
                    return doc;
                    
                }

                public Stream LoadFile(Uri href)
                {
                    throw new NotImplementedException();
                }
            }
            protected static string WrapListDocument =
                @"<?xml version=""1.0"" encoding=""utf-8""?>
<wraplist>
  <wrap name=""castle-dynamicproxy"" title=""Castle DynamicProxy"" version=""2.1.0"">
    <description>blah</description>
    <link rel=""package"" href=""wraps/castle-dynamicproxy-2.1.0.wrap"" />
    <depends>castle-core = 1.1.0</depends>
  </wrap>
  <wrap name=""castle-core"" title=""Castle core components"" version=""1.1.0"" hidden=""true"">
    <description>Castle core functionality</description>
    <link rel=""package"" href=""wraps/castle-core-1.1.0.wrap"" />
  </wrap>
  <wrap name=""nhibernate-core"" title=""NHibernate core module"" version=""2.1.2"" hidden=""true"">
    <description>
      The core nhibernate data
    </description>
    <link rel=""package"" href=""wraps/nhibernate-core-2.1.2.wrap"" />
  </wrap>
  <wrap name=""nhibernate-castle"" title=""NHibernate with Castle lazy loading"" version=""2.1.2"">
    <description>The core nhibernate data</description>
    <link rel=""package"" href=""wraps/nhibernate-core-2.1.2.wrap"" />
    <depends>nhibernate-core = 2.1.2</depends>
    <depends>castle-dynamicproxy = 2.1.0</depends>
  </wrap>
</wraplist>";

            protected IPackageRepository Repository;
        }
    }
}
