using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.InMemory;
using OpenWrap;
using OpenWrap.Build;
using OpenWrap.Build.PackageBuilders;
using OpenWrap.Runtime;
using OpenWrap.Testing;
using OpenWrap.Tests.Commands;

namespace Tests.Build.contexts
{
    public abstract class msbuild_builder : context
    {
        protected IEnumerable<BuildResult> Results;
        IFileSystem _fileSystem;
        protected IDirectory rootPath;
        string _text;
        int _returnCode;
        protected MSBuildPackageBuilder Builder;

        protected IEnumerable<ErrorBuildResult> SrcRelatedErrorResults
        {
            get
            {
                return Results.OfType<ErrorBuildResult>().Where(r => r.Message != null && r.Message.Contains("src"));
            }
        }


        public msbuild_builder()
        {
            _fileSystem = new InMemoryFileSystem();
            rootPath = _fileSystem.CreateDirectory("testdir");
        }

        protected void given_empty_directory()
        {
            rootPath.GetFile("test.wrapdesc").MustExist();
        }

        protected void given_file(string fileName)
        {
            rootPath.GetFile(fileName).MustExist();
        }
        protected void given_file(Func<IDirectory,IDirectory> dir, string fileName)
        {
            var currentDir = dir(_fileSystem.GetDirectory("testdir")).MustExist();
            currentDir.GetFile(fileName).MustExist();
        }
        protected void given_msbuild_result(int returnCode, string text)
        {
            _returnCode = returnCode;
            _text = text;
        }
        protected void when_building_package()
        {
            Results = CreateBuilder().Build().ToList();
        }

        protected void when_building(string project, params Action<MSBuildPackageBuilder>[] builderConfig)
        {
            CreateBuilder();
            Builder.Project = new[] { project };
            foreach (var builder in builderConfig) builder(Builder);
            Results = Builder.Build().ToList();
        }

        MSBuildPackageBuilder CreateBuilder()
        {
            return Builder = new SpyMSBuildPackageBuilder(_fileSystem,
                                             new InMemoryEnvironment(rootPath, rootPath)
                                             {
                                                 ExecutionEnvironment = new ExecutionEnvironment("AnyCPU", "net35")
                                             },
                                             new ResultParserStub(),
                                             _text,_returnCode);
        }


        class ResultParserStub : IFileBuildResultParser
        {
            public IEnumerable<FileBuildResult> Parse(string line)
            {
                yield break;
            }
        }
        class SpyMSBuildPackageBuilder : MSBuildPackageBuilder
        {
            readonly string _output;
            readonly int _returnCode;

            public SpyMSBuildPackageBuilder(IFileSystem fileSystem, IEnvironment environment, IFileBuildResultParser parser, string output, int returnCode) : base(fileSystem, environment, parser)
            {
                _output = output;
                _returnCode = returnCode;
            }

            protected override IProcess CreateProcess(string arguments)
            {
                return new InMemoryProcess(_output, _returnCode);
            }
        }
    }

    class InMemoryProcess : IProcess
    {
        readonly string _output;
        readonly int _returnCode;

        public InMemoryProcess(string output, int returnCode)
        {
            _output = output ?? string.Empty;
            _returnCode = returnCode;
        }

        public StreamReader StandardOutput
        {
            get { return new StreamReader(_output.ToUTF8Stream(), Encoding.UTF8); }
        }

        public int ExitCode
        {
            get { return _returnCode; }
        }

        public bool Start()
        {
            return true;
        }

        public void WaitForExit()
        {
        }

        public void SetEnvironmentVariable(string key, string value)
        {
        }
    }
}