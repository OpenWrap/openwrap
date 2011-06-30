using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.Resources;
using Tests;
using Tests.contexts;

namespace Tests.Reflection
{
    public class can_sign_unsigned_assembly : signing
    {
        public can_sign_unsigned_assembly()
        {
            given_assembly_of<can_sign_unsigned_assembly>("assembly.dll");
            given_key(Keys.openwrap);
            when_signing();
        }

        [Test]
        public void assembly_signing_is_valid()
        {
            then_assembly_signing_is_valid();
        }
    }
}