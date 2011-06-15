using System;
using Microsoft.Win32;
using NUnit.Framework;
using OpenWrap.Testing;
using OpenWrap.VisualStudio.Hooks;

namespace Tests.VisualStudio.addin_install
{
    public class installing_fresh : contexts.addin_installer
    {
        public installing_fresh()
        {
            given_empty_registry_for<TemporaryComAddIn>();

            when_installing<TemporaryComAddIn>("1.0.0.0");
        }

        [Test]
        public void versioned_directory_is_added()
        {
            InstallDir.GetDirectory("1.0.0.0").Exists.ShouldBeTrue();
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
    }
}