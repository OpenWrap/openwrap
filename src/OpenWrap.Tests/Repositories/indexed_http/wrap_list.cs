using System;
using System.IO;
using System.Xml.Linq;
using OpenFileSystem.IO.FileSystems.Local;
using OpenWrap.Repositories;
using OpenWrap.Repositories.FileSystem;
using OpenWrap.Repositories.Http;

namespace Tests.Repositories.indexed_http.context
{
    public abstract class wrap_list : OpenWrap.Testing.context
    {
        protected void given_repository()
        {
            Repository = new IndexedHttpRepository(LocalFileSystem.Instance, "remote", new InMemoryNavigator());
        }
        class InMemoryNavigator : IHttpRepositoryNavigator
        {
            public PackageFeed Index()
            {
                var doc = XDocument.Parse(WrapListDocument, LoadOptions.SetBaseUri);
                return doc.ParsePackageDocument();
            }

            public Stream LoadPackage(PackageEntry packageEntry)
            {
                throw new NotSupportedException();
            }

            public bool CanPublish
            {
                get { return false; }
            }

            public void PushPackage(string packageFileName, Stream packageStream)
            {
                throw new NotSupportedException();
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