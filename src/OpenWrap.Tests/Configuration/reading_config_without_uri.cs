using System;
using NUnit.Framework;
using OpenWrap.Testing;
using Tests.contexts;

namespace Tests.Configuration
{
    public class reading_config_without_uri : configuration<reading_config_without_uri>
    {
        public reading_config_without_uri()
        {
            when_loading_configuration();
        }

        [Test]
        public void error_is_raised()
        {
            Error.ShouldBeOfType<InvalidOperationException>();
        }
    }
}