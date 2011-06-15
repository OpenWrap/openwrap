using NUnit.Framework;
using OpenWrap.Testing;

namespace Tests.VisualStudio.addin_install
{
    public class upgrading : contexts.addin_installer
    {
        public upgrading()
        {
            given_empty_registry_for<TemporaryComAddIn>();
            given_install<TemporaryComAddIn>("1.0.0.0");
            when_installing<TemporaryComAddIn>("1.0.0.1");
        }

        [Test]
        public void versioned_directory_for_new_version_is_added()
        {
            InstallDir.GetDirectory("1.0.0.1").Exists.ShouldBeTrue();
        }

        [Test]
        public void file_is_copied()
        {
            InstallDir.GetDirectory("1.0.0.1").GetFile(FileName<TemporaryComAddIn>())
                .Exists.ShouldBeTrue();
        }

        [Test]
        public void registry_points_to_correct_file()
        {
            CodeBase<TemporaryComAddIn>().ShouldBe(
                InstallDir.GetDirectory("1.0.0.1").GetFile(FileName<TemporaryComAddIn>()).Path.ToFileUri());
        }
    }
}