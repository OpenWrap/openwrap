using NUnit.Framework;

namespace Testing.IO
{
    public class template_path_segment_specs : contexts.template_path_segment
    {
        [Test]
        public void var_name_is_correct()
        {
            Template("{source}", "source").ShouldHaveName("source");
        }
        [Test]
        public void simple_parameter_is_parsed()
        {
            Template("{source}", "src").ShouldHaveValue("source", "src");
        }
        [Test]
        public void parameter_with_value_is_parsed()
        {
            Template("{source: src}", "src")
                    .ShouldHaveName("source")
                    .ShouldHaveValue("source", "src");
            Template("{source: src}", "source").ShouldNotHaveName("source");
        }
        [Test]
        public void parameter_with_value_is_replaced_with_transform()
        {
            Template("{source: src=source}", "src").ShouldHaveValue("source", "source");
            Template("{source: src=source}", "source").ShouldNotHaveName("source");
        }
    }
}