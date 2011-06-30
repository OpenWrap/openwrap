using System;
using NUnit.Framework;
using OpenWrap.Testing;
using Tests.VisualStudio.contexts;

namespace Tests.VisualStudio.addin_install
{
    public class installing_fresh : addin_installer
    {
        public installing_fresh()
        {
            given_empty_registry_for<TemporaryComAddIn>();

            when_installing<TemporaryComAddIn>("1.0.0.0");
        }

        [Test]
        public void file_is_copied()
        {
            InstallDir.GetDirectory("1.0.0.0").GetFile(FileName<TemporaryComAddIn>())
                .Exists.ShouldBeTrue();
        }

        [Test]
        public void registry_points_to_correct_file()
        {
            CodeBase<TemporaryComAddIn>().ShouldBe(
                InstallDir.GetDirectory("1.0.0.0").GetFile(FileName<TemporaryComAddIn>()).Path.ToFileUri());
        }

        [Test]
        public void versioned_directory_is_added()
        {
            InstallDir.GetDirectory("1.0.0.0").Exists.ShouldBeTrue();
        }
    }
}