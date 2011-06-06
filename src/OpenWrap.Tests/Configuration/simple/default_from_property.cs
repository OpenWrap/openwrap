using NUnit.Framework;
using OpenWrap.Testing;
using Tests.contexts;

namespace Tests.Configuration.simple
{
    public class default_from_property : configuration<default_from_property>
    {
        static readonly default_from_property _def = new default_from_property();

        public default_from_property()
        {
            when_loading_configuration("nowhere");
        }

        public static default_from_property Default
        {
            get { return _def; }
        }

        [Test]
        public void default_is_returned()
        {
            Entry.ShouldBeSameInstanceAs(Default);
        }
    }
}