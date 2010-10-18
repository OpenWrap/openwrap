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
            when_loading_assembly_in_gac();
        }

        [Test]
        public void has_resolved()
        {
            result.SelectMany(x => x).ShouldHaveCountOf(1).First().Name.ShouldBe("System.Xml");
        }
    }
}
