using NUnit.Framework;
using OpenWrap.Commands.Errors;
using OpenWrap.Testing;

namespace Tests.Commands.remote.set
{
    public class remote_name_doesnt_exist : contexts.set_remote
    {
        public remote_name_doesnt_exist()
        {
            given_remote_config("primus");
            when_executing_command("canad -priority 200");
        }

        [Test]
        public void error_is_returned()
        {
            Results.ShouldHaveOne<UnknownRemoteName>()
                .Name.ShouldBe("canad");
        }
    }
}