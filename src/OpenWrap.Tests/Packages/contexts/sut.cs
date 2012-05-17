using OpenWrap.PackageModel;

namespace Tests.Packages.contexts
{
    public abstract class sut
    {
        public abstract IPackageInfo package { get; }
        public abstract void when_creating_package();
        public abstract void given_descriptor(params string[] content);
        public abstract void given_file(string fileName, string content);
    }
}