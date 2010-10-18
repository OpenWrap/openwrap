using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenWrap.Commands.Core;
using OpenWrap.Dependencies;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace OpenWrap.Tests.Dependencies
{
    public abstract class gac_resolve : Testing.context
    {
        protected bool result;

        protected void when_resolving(string partialName)
        {
            result =  GACResolve.InGAC(new ResolvedDependency
            {
                    Dependency = new PackageDependency { Name = partialName }
            });
        }
    }
}
