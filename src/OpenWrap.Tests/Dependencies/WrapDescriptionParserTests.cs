using NUnit.Framework;
using OpenWrap.Dependencies;
using OpenWrap.Testing;

namespace OpenWrap.Tests.Dependencies
{
    public class WrapDescriptionParserTests : context
    {
        [Test]
        public void Parsing_description_sets_Description_property()
        {
            var parser = new WrapDescriptionParser();
            var wrapDescriptor = new WrapDescriptor();
            parser.Parse("description test message", wrapDescriptor);
            wrapDescriptor.Description.ShouldBe("test message");
        }

        [Test]
        public void Parsing_empty_description_sets_Description_to_empty_string()
        {
            var parser = new WrapDescriptionParser();
            var wrapDescriptor = new WrapDescriptor();
            parser.Parse("description", wrapDescriptor);
            wrapDescriptor.Description.ShouldBe("");
        }

        [Test]
        public void Parsing_whitespace_description_sets_Description_to_empty_string()
        {
            var parser = new WrapDescriptionParser();
            var wrapDescriptor = new WrapDescriptor();
            parser.Parse("description  ", wrapDescriptor);
            wrapDescriptor.Description.ShouldBe("");
        }

        [Test]
        public void Parsing_description_trims_whitespace()
        {
            var parser = new WrapDescriptionParser();
            var wrapDescriptor = new WrapDescriptor();
            parser.Parse("description  \t test message   \t", wrapDescriptor);
            wrapDescriptor.Description.ShouldBe("test message");
        }
    }
}
