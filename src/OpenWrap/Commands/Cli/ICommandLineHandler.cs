using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenWrap.Commands.Cli
{
    public interface ICommandLineHandler
    {
        ICommandDescriptor Execute(ref string input);
    }
}
