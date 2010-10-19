using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using OpenWrap.Repositories;

namespace OpenWrap.Commands
{
        

        public GacConflict(IPackageInfo package, AssemblyName assembly)
            : base(string.Format(MESSAGE,package.FullName, assembly.FullName))
        {
            const string MESSAGE = "Package '{0}' contains assembly '{1}' that is already in the Global Assembly Cache. For this assembly to be used, you need to remove this assembly from the gac.";
        }
    }
}
