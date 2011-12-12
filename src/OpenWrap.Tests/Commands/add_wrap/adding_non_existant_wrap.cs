using System;
using NUnit.Framework;
using OpenWrap.Commands;
using OpenWrap.Commands.Wrap;
using OpenWrap.Testing;
using Tests.Commands.contexts;

namespace Tests.Commands.add_wrap
{
    class adding_non_existant_wrap : command<AddWrapCommand>
    {
        public adding_non_existant_wrap()
        {
            given_currentdirectory_package("sauron", "1.0.0");
            when_executing_command("-Name saruman");
        }
        [Test]
        public void package_installation_is_unsuccessfull()
        {
            Results.ShouldHaveAtLeastOne(x => CommandOutputExtensions.Success(x) == false);
        }
    }
}