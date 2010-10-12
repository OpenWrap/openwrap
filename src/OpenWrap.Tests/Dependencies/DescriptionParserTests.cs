using NUnit.Framework;
using OpenWrap.Dependencies;
using OpenWrap.Testing;

namespace OpenWrap.Tests.Dependencies
{
    public class DescriptionParserTests : context
    {
        [Test]
        public void Parsing_description_sets_Description_property()
        {
            var parser = new DescriptionParser();
            var wrapDescriptor = new PackageDescriptor();
            parser.Parse("description: test message", wrapDescriptor);
            wrapDescriptor.Description.ShouldBe("test message");
        }

        [Test]
        public void Parsing_empty_description_sets_Description_to_empty_string()
        {
            var parser = new DescriptionParser();
            var wrapDescriptor = new PackageDescriptor();
            parser.Parse("description:", wrapDescriptor);
            wrapDescriptor.Description.ShouldBe("");
        }

        [Test]
        public void Parsing_whitespace_description_sets_Description_to_empty_string()
        {
            var parser = new DescriptionParser();
            var wrapDescriptor = new PackageDescriptor();
            parser.Parse("description : ", wrapDescriptor);
            wrapDescriptor.Description.ShouldBe("");
        }

        [Test]
        public void Parsing_description_trims_whitespace()
        {
            var parser = new DescriptionParser();
            var wrapDescriptor = new PackageDescriptor();
            parser.Parse("description : \t test message   \t", wrapDescriptor);
            wrapDescriptor.Description.ShouldBe("test message");
        }
    }
}
