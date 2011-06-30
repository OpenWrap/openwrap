// ReSharper disable InconsistentNaming
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenWrap.VisualStudio.Interop
{
    public enum OLECMDF : uint
    {
            SUPPORTED = 1,
            ENABLED = 2,
            LATCHED = 4,
            NINCHED = 8,
            INVISIBLE = 16
    }
}
// ReSharper restore InconsistentNaming