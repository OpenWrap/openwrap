using NUnit.Framework;
using OpenWrap.Testing;
using Tests.contexts;

namespace Tests.Configuration.simple
{
    public class default_from_field : configuration<default_from_field>
    {
        public static readonly default_from_field Default = new default_from_field();

        public default_from_field()
        {
            when_loading_configuration("nowhere");
        }

        [Test]
        public void default_is_returned()
        {
            Entry.ShouldBeSameInstanceAs(Default);
        }
    }
}