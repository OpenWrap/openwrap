using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.contexts;

namespace package_descriptor_specs
{
    public class removing_a_multiline_value : descriptor
    {
        public removing_a_multiline_value()
        {
            given_descriptor("depends: ered-luin", "depends: ered-mithrin", "depends: ered-lithui");
            Descriptor.Dependencies.Remove(Descriptor.Dependencies.First(x => x.Name.EqualsNoCase("ered-mithrin")));

            when_writing();
        }
        [Test]
        public void order_of_remaining_lines_is_preserved()
        {
            descriptor_should_be("depends: ered-luin\r\ndepends: ered-lithui");
        }

    }
}
