using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenWrap.Testing;
using OpenWrap.Tests.Dependencies;

namespace OpenWrap.Tests.Commands.Core
{
    public class gac_resolve_existing : gac_resolve
    {

        public gac_resolve_existing()
        {
            when_resolving("System");
        }

        [Test]
        public void has_resolved()
        {
            result.ShouldBeTrue();
        }
    }

    public class gac_resolve_nonexisting : gac_resolve
    {

        public gac_resolve_nonexisting()
        {
            when_resolving("ThisAssemblyAlmostCertainlyDoesntExistLordOfTheRingsReference");
        }

        [Test]
        public void has_not_resolved()
        {
            result.ShouldBeFalse();
        }
    }
}
