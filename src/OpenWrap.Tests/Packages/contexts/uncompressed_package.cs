using System;
using System.Collections.Generic;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.InMemory;
using OpenWrap;
using OpenWrap.IO;
using OpenWrap.IO.Packaging;
using OpenWrap.PackageManagement.Packages;
using OpenWrap.PackageModel;
using OpenWrap.Testing;
using Tests.Packages.Uncompressed;

namespace Tests.Packages.contexts
{
    public abstract class sut
    {
        public abstract IPackageInfo package { get; }
        public abstract void when_creating_package();
        public abstract void given_descriptor(params string[] content);
        public abstract void given_file(string fileName, string content);
    }

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