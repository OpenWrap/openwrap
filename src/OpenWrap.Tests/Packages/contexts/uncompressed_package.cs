using System;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.InMemory;
using OpenWrap;
using OpenWrap.IO;
using OpenWrap.PackageManagement.Packages;
using OpenWrap.PackageModel;
using OpenWrap.Testing;

namespace Tests.Packages.contexts
{
    public class uncompressed_package : sut
    {
        public override IPackageInfo package { get { return _package; } }
        UncompressedPackage _package;
        string _descriptor;
        InMemoryFileSystem _fs = new InMemoryFileSystem();
        ITemporaryFile _packageFile;
        ITemporaryDirectory _packageDirectory;

        public uncompressed_package()
        {
            _packageFile = _fs.CreateTempFile();
            _packageDirectory = _fs.CreateTempDirectory();
        }

        public override void when_creating_package()
        {
            this._package = new UncompressedPackage(NullRepository.Instance, _packageFile, _packageDirectory);
        }

        public override void given_descriptor(params string[] content)
        {
            _packageDirectory.GetFile("package.wrapdesc").WriteString(content.JoinString("\r\n"));
        }

        public override void given_file(string fileName, string content)
        {
            _packageDirectory.GetFile(fileName).WriteString(content);
        }
    }
}