using NUnit.Framework;
using OpenWrap.Commands.Remote;
using OpenWrap.Commands.Remote.Messages;
using OpenWrap.Testing;
using Tests.Commands.contexts;

namespace Tests.Commands.remote.list.detailed
{
    class list_remote_with_fetch_and_credentials : command<ListRemoteCommand>
    {
        public list_remote_with_fetch_and_credentials()
        {
            given_remote_config("sauron", "[memory]Sauron's repository", fetchUsername: "sauron", fetchPassword: "secretely_loves_gandalf");
            given_remote_factory_memory();
            when_executing_command("-detailed");
        }

        [Test]
        public void entry_is_printed()
        {
            Results.ShouldHaveOne<RemoteRepositoryDetailedData>()
                .ToString().ShouldBe("  1 sauron     [fetch]\r\n" +
                                     "               name    : Sauron's repository\r\n" +
                                     "               type    : memory\r\n" +
                                     "               token   : [memory]Sauron's repository\r\n" +
                                     "               username: sauron\r\n" +
                                     "               password: secretely_loves_gandalf\r\n"
                );
        }
    }
}