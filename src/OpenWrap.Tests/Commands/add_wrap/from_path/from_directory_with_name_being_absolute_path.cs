using NUnit.Framework;

namespace Tests.Commands.add_wrap.from_path
{
    class from_directory_with_name_being_absolute_path : contexts.add_wrap
    {
        public from_directory_with_name_being_absolute_path()
        {
            given_current_directory(@"c:\\rohan");

            when_executing_command(@"c:\sauron\myFile.wrap", "-from", "c:\\mordor");
        }

        [Test]
        public void an_error_is_recorded()
        {
            Results.ShouldHaveError();
        }
    }
}