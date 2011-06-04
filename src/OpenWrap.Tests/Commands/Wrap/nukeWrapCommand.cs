using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenWrap.Commands.contexts;
using OpenWrap.Commands.Remote;
using OpenWrap.Commands.Wrap;
using Tests.Commands.contexts;

namespace OpenWrap.Tests.Commands.Wrap
{
    class when_nuking_an_existing_wrap : command<NukeWrapCommand>
    {
        public when_nuking_an_existing_wrap()
        {
        }
    }
}
