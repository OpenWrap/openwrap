using NUnit.Framework;
using OpenWrap.Commands;
using OpenWrap.Commands.contexts;
using OpenWrap.Commands.Wrap;
using OpenWrap.Repositories;
using OpenWrap.Testing;
using Tests.Commands.contexts;

namespace Tests.Commands.add_wrap
{
    class adding_wrap_with_incompatible_arguments : command<AddWrapCommand>
    {
        public adding_wrap_with_incompatible_arguments()
        {
            given_project_repository(new InMemoryRepository("Project repository"));

            when_executing_command("-System -Project");
        }
        [Test]
        public void results_in_an_error()
        {
            Results.ShouldHaveAtLeastOne(x => CommandOutputExtensions.Success(x) == false);
        }
    }
}