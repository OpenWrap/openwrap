using System;
using NUnit.Framework;
using OpenWrap.Testing;
using Tests.contexts;

namespace Tests.Configuration.simple
{
    public class save_null_does_nothing : configuration<save_null_does_nothing>
    {
        public save_null_does_nothing()
        {
            given_configuration(null);
            when_saving_configuration("nowhere");
        }

        [Test]
        public void entry_isnt_saved()
        {
            ConfigurationDirectory.GetFile("nowhere").Exists.ShouldBeFalse();
        }
    }
}