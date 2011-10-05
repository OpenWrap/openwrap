using NUnit.Framework;
using OpenWrap;

namespace Tests.Commands.add_wrap
{
    public class adding_major_version : contexts.add_wrap
    {
        public adding_major_version()
        {
            given_file_based_project_repository();

            given_remote_package("sauron", "1.0.0".ToVersion());
            given_remote_package("sauron", "2.0.0".ToVersion());
            when_executing_command("sauron -version 1");
        }

        [Test]
        public void fails()
        {
            Results.ShouldHaveError();
        }
    }
}