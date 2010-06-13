using System;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenWrap.Commands.Wrap;
using OpenWrap.Testing;

namespace OpenWrap.Tests.Commands
{
    class adding_wrap_from_local_package_to_local_repository : context.command_context<AddWrapCommand>
    {
        public adding_wrap_from_local_package_to_local_repository()
        {
            given_dependency("depends mordor");
            given_project_repository();

            given_remote_package("sauron", new Version(1, 0, 0));
            given_remote_package("rings-of-power", new Version(1,0,0), "depends sauron");

            when_executing_command();
        }

    }
}
