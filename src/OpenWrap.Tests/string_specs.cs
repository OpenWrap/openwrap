using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenWrap.PackageModel.Serialization;
using OpenWrap.Testing;

namespace OpenWrap.Tests
{
    public class string_specs : context
    {
        [Test]
        public void lines_are_unfolded()
        {
            "depends: openwrap\r\ndepends:openwrap2 \r\n\r\ndepends: openwrap3 "
                    .GetUnfoldedLines()
                    .ShouldHaveCountOf(3)
                    .Check(x => x.ElementAt(0).ShouldBe("depends: openwrap"))
                    .Check(x => x.ElementAt(1).ShouldBe("depends:openwrap2"))
                    .Check(x => x.ElementAt(2).ShouldBe("depends: openwrap3"));

        }
    }
}
