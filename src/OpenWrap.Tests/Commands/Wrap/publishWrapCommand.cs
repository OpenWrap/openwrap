using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenRasta.Wrap.Tests.Dependencies.context;
using OpenWrap.Commands.Errors;
using OpenWrap.Commands.Wrap;
using OpenWrap.Repositories;
using OpenWrap.Tests;
using OpenWrap.Testing;
using OpenWrap.Tests.Commands.context;

namespace publish_wrap_specifications
{
    public class publishing_to_unknown_remote : context.publish_wrap
    {
        public publishing_to_unknown_remote()
        {
            given_currentdirectory_package("sauron", "1.0.0.123");
            when_executing_command("-remote", "mordor","-path","sauron-1.0.0.123.wrap");
        }

        [Test]
        public void command_fails()
        {
            Results.ShouldContain<UnknownRemoteRepository>();
        }
    }
    public class publishing_unknown_file : context.publish_wrap
    {
        public publishing_unknown_file()
        {
            given_remote_repository("mordor");
            when_executing_command("-remote", "mordor", "-path", "sauron-1.0.0.123.wrap");
        }
        [Test]
        public void an_unknown_file_error_is_triggered()
        {
            Results.ShouldContain<FileNotFound>();
        }
    }
    public class publishing_file_to_remote : context.publish_wrap
    {
        public publishing_file_to_remote()
        {
            given_remote_repository("mordor");
            given_current_directory_repository(new CurrentDirectoryRepository());
            given_currentdirectory_package("sauron", "1.0.0.123");

            when_executing_command("-remote", "mordor", "-path", "sauron-1.0.0.123.wrap");
        }
        [Test]
        public void the_package_is_published()
        {
            package_is_in_repository(Environment.RemoteRepositories.First(x=>x.Name == "mordor"), "sauron", new Version("1.0.0.123"));
        }
    }
    //public class publishing_unknown_file
    namespace context
    {
        public class publish_wrap : command_context<PublishWrapCommand>
        {
            
        }
    }
}
