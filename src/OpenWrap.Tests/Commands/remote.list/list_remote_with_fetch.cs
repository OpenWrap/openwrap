using NUnit.Framework;
using OpenWrap.Commands.Remote;
using OpenWrap.Commands.Remote.Messages;
using OpenWrap.Testing;
using Tests.Commands.contexts;

namespace Tests.Commands.remote.list
{
    class list_remote_with_fetch : command<ListRemoteCommand>
    {
        public list_remote_with_fetch()
        {
            given_remote_config("sauron");
            when_executing_command();
        }

        [Test]
        public void data_is_correct()
        {
            Results.ShouldHaveOne<RemoteRepositoryData>()
                .Check(x => x.Name.ShouldBe("sauron"))
                .Check(x => x.Fetch.ShouldBeTrue())
                .Check(x => x.Publish.ShouldBeFalse())
                .ToString().ShouldBe("  1 sauron     [fetch]");
        }
    }
}