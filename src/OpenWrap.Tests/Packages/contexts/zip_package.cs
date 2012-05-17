using System.Collections.Generic;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.InMemory;
using OpenWrap;
using OpenWrap.IO.Packaging;
using OpenWrap.PackageManagement.Packages;
using OpenWrap.PackageModel;

namespace Tests.Packages.contexts
{
    public class zip_package : sut
    {
        public override IPackageInfo package { get { return _package; } }
        ZipFilePackage _package;
        string _descriptor;
        InMemoryFileSystem _fs = new InMemoryFileSystem();
        ITemporaryFile _packageFile;
        ITemporaryDirectory _packageDirectory;
        string[] _descriptorContent;
        List<PackageContent> _content = new List<PackageContent>();


        public zip_package()
        {
            _packageFile = _fs.CreateTempFile();
        }

        public override void when_creating_package()
        {
            Packager.NewFromFiles(_packageFile, _content);
            _package = new ZipFilePackage(_packageFile);

        }

        public override void given_descriptor(params string[] content)
        {
            _content.Add(new PackageContent { FileName = "package.wrapdesc", Stream = () => content.JoinString("\r\n").ToUTF8Stream() });
        }

        public override void given_file(string fileName, string content)
        {
            _content.Add(new PackageContent { FileName = fileName, Stream = () => content.JoinString("\r\n").ToUTF8Stream() });
        }
    }
}