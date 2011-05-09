using NUnit.Framework;
using OpenWrap.Commands.Errors;
using OpenWrap.Tests;
using Tests.Commands.contexts;

namespace publish_wrap_specifications
{
    public class publishing_unknown_file : publish_wrap
    {
        public publishing_unknown_file()
        {
            given_remote_repository("mordor");
            when_executing_command("-remote mordor -path sauron-1.0.0.123.wrap");
        }
        [Test]
        public void an_unknown_file_error_is_triggered()
        {
            Results.ShouldContain<FileNotFound>();
        }
    }
}