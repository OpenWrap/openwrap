using NUnit.Framework;
using OpenWrap.Testing;

namespace Tests.VisualStudio.addin_install
{
    public class alread_up_to_date : contexts.addin_installer
    {
        public alread_up_to_date()
        {
            given_empty_registry_for<TemporaryComAddIn>();
            given_install<TemporaryComAddIn>("1.0.0.1");
            when_installing<TemporaryComAddIn>("1.0.0.0");
        }

        [Test]
        public void versioned_directory_for_old_version_not_created()
        {
            InstallDir.GetDirectory("1.0.0.0").Exists.ShouldBeFalse();
        }

        [Test]
        public void registry_points_to_correct_file()
        {
            CodeBase<TemporaryComAddIn>().ShouldBe(
                InstallDir.GetDirectory("1.0.0.1").GetFile(FileName<TemporaryComAddIn>()).Path.ToFileUri());
        }
    }
}