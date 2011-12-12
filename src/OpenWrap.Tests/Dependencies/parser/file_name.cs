using OpenWrap.PackageModel;
using OpenWrap.Testing;

namespace Tests.Dependencies.parser
{
    public class file_name : context
    {
        public void version_is_parsed()
        {
            PackageNameUtility.GetVersion("openrasta-2.0.0").ShouldBe("2.0.0");
            PackageNameUtility.GetName("openrasta-2.0.0").ShouldBe("openrasta");
        }

        public void invalid_version_is_ignored()
        {
            PackageNameUtility.GetVersion("openrasta-xxx").ShouldBeNull();
            PackageNameUtility.GetName("openrasta-xxx").ShouldBe("openrasta-xxx");
        }
    }
}