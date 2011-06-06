using System;
using NUnit.Framework;
using OpenWrap.Collections;
using OpenWrap.Testing;
using Tests.contexts;

namespace Tests.Configuration.dictionaries
{
    public class wirting_to_readonly_dictionary : configuration<wirting_to_readonly_dictionary.ThrowingDictionary>
    {
        public wirting_to_readonly_dictionary()
        {
            given_configuration_file("place", "[DateTime key]\r\nvalue: value");
            when_loading_configuration("place");
        }

        [Test]
        public void error_bubbles()
        {
            Error.ShouldBeOfType<NotSupportedException>();
        }

        public class ThrowingDictionary : IndexedDictionary<string, DateTime>
        {
            public ThrowingDictionary() :
                base(x => x.ToString(), ThrowOnKey)
            {
            }

            static void ThrowOnKey(string arg1, DateTime arg2)
            {
                throw new NotSupportedException();
            }
        }
    }
}