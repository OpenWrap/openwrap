﻿using System;
using System.Linq;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.Repositories;

namespace Tests.Commands.publish_wrap
{
    public class publishing_file_to_remote : contexts.publish_wrap
    {
        public publishing_file_to_remote()
        {
            given_remote_repository("mordor");
            given_current_directory_repository(new CurrentDirectoryRepository());
            given_currentdirectory_package("sauron", "1.0.0.123");

            when_executing_command("-remote mordor -path sauron-1.0.0+123.wrap");
        }
        [Test]
        public void the_package_is_published()
        {
            package_is_in_repository(RemoteRepositories.First(x=>x.Name == "mordor"), "sauron", "1.0.0+123".ToSemVer());
        }
    }
}