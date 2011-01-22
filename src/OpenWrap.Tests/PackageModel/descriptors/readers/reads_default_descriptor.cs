using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenWrap.PackageModel.descriptors.readers
{
    public class reads_default_descriptor : contexts.descriptor_readers
    {
        public reads_default_descriptor()
        {
            given_descriptor("name: one-ring");
            when_reading_all();
        }
    }
}
