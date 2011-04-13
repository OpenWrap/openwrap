using NUnit.Framework;
using OpenWrap.Testing;

namespace OpenWrap.Tests.Extensions
{
    public class HumpsMatching : context
    {
        [Test]
        public void humps_are_matched()
        {
            "bg".MatchesHumps("Baggins").ShouldBeTrue();

        }
        [Test]
        public void humps_are_matched_case_insensitively()
        {
            "bG".MatchesHumps("Baggins").ShouldBeTrue();
        }
        [Test]
        public void humps_are_matched_on_repetition()
        {
            "bgi".MatchesHumps("Baggins").ShouldBeTrue();
        }
        [Test]
        public void humps_dont_match_incompatible_strings()
        {
            "bgk".MatchesHumps("Baggins").ShouldBeFalse();
        }

        [Test]
        public void humps_dont_match_if_first_char_not_matched()
        {
            "aggins".MatchesHumps("Baggins").ShouldBeFalse();
        }
    }
    public class HumpsSelect : context
    {
        [Test]
        public void NAME()
        {
            
        }
    }
}