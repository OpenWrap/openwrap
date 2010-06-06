using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenRasta.Wrap.Tests.Dependencies.context;
using OpenWrap.Build.Services;
using OpenWrap.Commands;
using OpenWrap.Commands.Wrap;
using OpenWrap.Dependencies;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace OpenWrap.Tests.Commands
{
    class when_creating_new_folder_structure : context.command_context<NewWrapCommand>
    {
        public when_creating_new_folder_structure()
        {
        }

    }
}
