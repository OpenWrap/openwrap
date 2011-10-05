using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenFileSystem.IO;
using OpenWrap.IO;
using OpenWrap.IO.Packaging;
using OpenWrap.PackageModel;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace Tests.Repositories.folder.locking
{
    public class not_enabled_by_default : contexts.folder
    {
        public not_enabled_by_default()
        {
            given_folder_repository();
        }

        [Test]
        public void feature_is_not_supported()
        {
            repository.Feature<ISupportLocking>().ShouldBeNull();
        }
    }
}