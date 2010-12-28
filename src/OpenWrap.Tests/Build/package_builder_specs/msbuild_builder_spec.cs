using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.InMemory;
using OpenWrap.Build;
using OpenWrap.Build.PackageBuilders;
using OpenWrap.Runtime;
using OpenWrap.Tests.Build.package_builder_specs.context;
using OpenWrap.Tests.Commands;

namespace OpenWrap.Tests.Build.package_builder_specs
{
    public class msbuild_builder_spec : msbuild_builder
    {
        [Test]
        public void does_require_src_folder_for_implicit_projects()
        {
            given_a_working_dir_without_src_folder();
            given_an_existing_file_in_working_dir("test1.csproj");

            when_building_package();

            should_contain_error_about_src();
        }

        [Test]
        public void doesnt_require_src_folder_for_explicit_projects()
        {
            given_a_working_dir_without_src_folder();
            given_an_existing_file_in_working_dir("test1.csproj");

            when_building_package_for("test1.csproj");

            should_not_contain_error_about_src();
        }
    }

    namespace context
    {
        public abstract class msbuild_builder
        {
            IEnumerable<BuildResult> _buildResults;
            IFileSystem _fileSystem;
            IDirectory _testDir;

            IEnumerable<ErrorBuildResult> SrcRelatedErrorResults
            {
                get
                {
                    return (from r in _buildResults
                            where r is ErrorBuildResult && r.Message != null && r.Message.Contains("src")
                            select (ErrorBuildResult)r);
                }
            }

            [SetUp]
            public void Init()
            {
                _testDir = null;
                _fileSystem = new InMemoryFileSystem();
            }

            protected void given_a_working_dir_without_src_folder()
            {
                Assert.IsNull(_testDir, "Test directory already initialized");
                _testDir = _fileSystem.CreateDirectory("testdir");
                _testDir.GetFile("test.wrapdesc").MustExist();
            }

            protected void given_an_existing_file_in_working_dir(string fileName)
            {
                Assert.IsNotNull(_testDir, "Working dir was not initialized");
                _testDir.GetFile(fileName).MustExist();
            }

            protected void should_contain_error_about_src()
            {
                Assert.IsNotNull(_buildResults, "Build results were not initialized");
                var srcErrors = SrcRelatedErrorResults.ToList();
                Assert.IsNotEmpty(srcErrors, "Got build error: {0}", (srcErrors.FirstOrDefault() ?? new ErrorBuildResult("no error?!")).Message);
            }

            protected void should_not_contain_error_about_src()
            {
                Assert.IsNotNull(_buildResults, "Build results were not initialized");
                var srcErrors = SrcRelatedErrorResults.ToList();
                Assert.IsEmpty(srcErrors, "Got build error: {0}", (srcErrors.FirstOrDefault() ?? new ErrorBuildResult("no error?!")).Message);
            }

            protected void when_building_package()
            {
                _buildResults = CreateBuilder().Build();
            }

            protected void when_building_package_for(string project)
            {
                var builder = CreateBuilder();
                builder.Project = new[] { project };
                _buildResults = builder.Build();
            }

            MSBuildPackageBuilder CreateBuilder()
            {
                return new MSBuildPackageBuilder(_fileSystem,
                                                 new InMemoryEnvironment(_testDir, _testDir)
                                                 {
                                                         ExecutionEnvironment = new ExecutionEnvironment("AnyCPU", "net-3.5")
                                                 },
                                                 new ResultParserStub());
            }


            class ResultParserStub : IFileBuildResultParser
            {
                public IEnumerable<FileBuildResult> Parse(string line)
                {
                    yield break;
                }
            }
        }
    }
}