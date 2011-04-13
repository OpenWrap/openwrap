using NUnit.Framework;
using OpenWrap.Testing;

namespace OpenWrap.Tests.Extensions
{
    public class CamelToSpacedName : context
    {
        [Test]
        public void should_not_touch_all_lowers()
        {
            "iaminlower".CamelToSpacedName().ShouldBe("iaminlower");
        }

        [Test]
        public void should_decamelcase_a_simple_string_correctly()
        {
            "IAmInCamelCase".CamelToSpacedName().ShouldBe("I am in camel case");
        }

        [Test]
        public void should_decamelcase_a_leading_lower_string_correctly()
        {
            "iAmInCamelCase".CamelToSpacedName().ShouldBe("i am in camel case");
        }
    }
}