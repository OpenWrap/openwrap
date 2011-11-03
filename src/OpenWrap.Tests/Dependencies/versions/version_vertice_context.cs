using System;
using OpenWrap;
using OpenWrap.PackageModel;
using OpenWrap.Testing;

namespace Tests.Dependencies.versions
{
    public abstract class version_vertice_context
    {
        protected void should_match(string versionvertice, string version)
        {
            CreateVertex(versionvertice)
                .IsCompatibleWith(version.ToVersion()).ShouldBeTrue();
        }

        protected void should_not_match(string versionvertice, string version)
        {
            CreateVertex(versionvertice)
                .IsCompatibleWith(version.ToVersion()).ShouldBeFalse();
        }

        protected abstract VersionVertex CreateVertex(string versionvertice);
    }
}
