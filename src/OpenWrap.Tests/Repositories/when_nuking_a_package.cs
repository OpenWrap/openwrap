using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;
using OpenWrap.Testing;

namespace Tests.Repositories
{
    public class when_nuking_a_package : context.indexed_folder_repository
    {
        public when_nuking_a_package()
        {
            given_file_system(@"c:\nuke");
            given_indexed_repository(@"c:\nuke\repository");
            given_published_package("pharrell", "1.0.0.0");
            when_nuking_package("pharrell", "1.0.0.0");
        }

        [Test]
        public void index_document_contains_nuked_attribute()
        {
            (from XElement node in IndexDocument.Descendants("wrap")
             where node.Attribute("name").Value.Equals("pharrell") &&
                   node.Attribute("version").Value.Equals("1.0.0.0") &&
                   node.Attribute("nuked").Value.Equals("true")
             select node)
                .ShouldHaveCountOf(1);
        }
        [Test]
        public void returned_packageinfo_is_marked_as_nuked()
        {
            Repository.PackagesByName["pharrell"]
                .Where(p => p.Version.ToString().Equals("1.0.0.0"))
                .FirstOrDefault()
                .ShouldNotBeNull()
                .Nuked
                .ShouldBeTrue();

        }

    }
}