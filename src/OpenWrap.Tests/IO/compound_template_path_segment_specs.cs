using NUnit.Framework;
using Testing.contexts;

namespace OpenWrap.Tests.IO
{
    public class compound_template_path_segment_specs : template_path_segment
    {
        [Test]
        public void can_have_prefix()
        {
            Template("begin{source}", "beginning")
                .ShouldHaveName("source")
                .ShouldHaveValue("source", "ning");
        }
        [Test]
        public void can_use_wildcard_prefix()
        {
            Template("*{source: tests=course}", "how about some tests")
                    .ShouldHaveName("source")
                    .ShouldHaveValue("source", "course");

        }

        [Test]
        public void can_have_suffix()
        {
            Template("{source}end", "westend")
                    .ShouldHaveName("source")
                    .ShouldHaveValue("source", "west");
        }
        [Test]
        public void can_be_surrounded()
        {
            Template("begin{source}end", "beginningistheend")
                    .ShouldHaveName("source")
                    .ShouldHaveValue("source", "ningisthe");

        }
        [Test]
        public void can_have_multiple_values()
        {
            Template("{source}.{destination}.csproj", "OpenWrap.Somewhere.csproj")
                    .ShouldHaveValue("source", "OpenWrap")
                    .ShouldHaveValue("destination", "Somewhere");
        }
    }
}