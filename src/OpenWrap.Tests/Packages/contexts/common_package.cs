using OpenWrap.PackageModel;

namespace Tests.Packages.contexts
{
    public abstract class common_package<TSut> where TSut: sut, new()
    {
        TSut _sut;
        protected IPackageInfo package { get { return _sut.package; } }
        public common_package()
        {
            _sut = new TSut();
        }
        protected void when_creating_package()
        {
            _sut.when_creating_package();
        }
        protected void given_descriptor(params string[] content)
        {
            _sut.given_descriptor(content);
        }
        protected void given_file(string fileName, string content)
        {
            _sut.given_file(fileName, content);

        }
    }
}