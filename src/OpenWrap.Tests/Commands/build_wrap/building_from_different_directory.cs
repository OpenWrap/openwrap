using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.Commands.contexts;
using OpenWrap.Commands.Wrap;
using OpenWrap.IO;
using OpenWrap.Testing;
using Tests.Commands.contexts;

namespace Tests.Commands.build_wrap
{
    class building_from_different_directory : command<BuildWrapCommand>
    {
        public building_from_different_directory()
        {

            given_current_directory(@"c:\current");
            given_file(@"c:\other\myPackage.wrapdesc", Content("name: myPackage\r\nversion: 1.0\r\nbuild: none"));
            when_executing_command(@"-from c:\other");
        }

        [Test]
        public void command_is_successful()
        {
            Results.ShouldHaveNoError();
        }
        [Test]
        public void package_is_created_in_current_directory()
        {
            FileSystem.GetFile("myPackage-1.0.wrap").Exists.ShouldBeTrue();
        }

        [Test]
        public void package_has_content()
        {
            FileSystem.GetFile("myPackage-1.0.wrap").Size.ShouldBeGreaterThan(0);
        }

        Stream Content(string content)
        {
            var ms = new MemoryStream();
            ms.Write(Encoding.UTF8.GetBytes(content));
            ms.Position = 0;
            return ms;
        }
    }
}
