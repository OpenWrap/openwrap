using NUnit.Framework;
using OpenWrap.Commands.contexts;
using OpenWrap.Testing;
using Tests.Commands.contexts;

namespace OpenWrap.Tests.Commands
{
    public class adding_anchored_dependency : add_wrap
    {
        public adding_anchored_dependency()
        {
            given_file_based_project_repository();

            given_system_package("sauron", "1.0.0");

            when_executing_command("sauron", "-anchored");
        }
        [Test]
        public void link_is_created()
        {
            ProjectRepositoryDir.GetDirectory("sauron")
                    .Check(x=>x.Exists.ShouldBeTrue())
                    .Check(x=>x.IsHardLink.ShouldBeTrue());
        }
        [Test]
        public void link_points_to_correct_path()
        {
            ProjectRepositoryDir.GetDirectory("sauron")
                    .Target.ShouldBe(ProjectRepositoryDir.GetDirectory("_cache").GetDirectory("sauron-1.0.0"));
        }
    }
}