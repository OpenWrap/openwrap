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
    public class HumpsMatching : context
    {
        [Test]
        public void humps_are_matched()
        {
            "g".MatchesHumps("Baggins").ShouldBeTrue();

        }
        [Test]
        public void humps_are_matched_case_insensitively()
        {
            "G".MatchesHumps("Baggins").ShouldBeTrue();
        }
        [Test]
        public void humps_are_matched_on_repetition()
        {
            "gi".MatchesHumps("Baggins").ShouldBeTrue();
        }
        [Test]
        public void humps_dont_match_incompatible_strings()
        {
            "gk".MatchesHumps("Baggins").ShouldBeFalse();
        }
    }
}