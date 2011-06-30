using NUnit.Framework;
using OpenWrap.Commands.Remote;
using OpenWrap.Commands.Remote.Messages;
using OpenWrap.Testing;
using Tests.Commands.contexts;

namespace Tests.Commands.remote.list
{
    class list_remote_with_muiltiple_publish : command<ListRemoteCommand>
    {
        public list_remote_with_muiltiple_publish()
        {
            given_remote_config("sauron", fetchToken: null, publishTokens: new[] { "[memory]somewhere", "[memory]over-the-rainbow" });
            when_executing_command();
        }

        [Test]
        public void entry_is_printed()
        {
            Results.ShouldHaveOne<RemoteRepositoryData>()
                .Check(x => x.Name.ShouldBe("sauron"))
                .Check(x => x.Fetch.ShouldBeFalse())
                .Check(x => x.Publish.ShouldBeTrue());
        }
    }
}