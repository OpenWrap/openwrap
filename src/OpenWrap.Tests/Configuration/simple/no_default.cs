using NUnit.Framework;
using OpenWrap.Testing;
using Tests.contexts;

namespace Tests.Configuration.simple
{
    public class no_default : configuration<no_default>
    {
        public no_default()
        {
            when_loading_configuration("nowhere");
        }

        [Test]
        public void nothing_is_returned()
        {
            Entry.ShouldBeNull();
        }
    }
}