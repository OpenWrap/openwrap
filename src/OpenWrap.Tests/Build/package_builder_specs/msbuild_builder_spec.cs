using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.InMemory;
using OpenWrap.Build;
using OpenWrap.Build.PackageBuilders;
using OpenWrap.Runtime;
using OpenWrap.Testing;
using OpenWrap.Tests.Commands;

namespace OpenWrap.Tests.Build.package_builder_specs
{
    public class building_for_implicit_projects_without_source_folder : contexts.msbuild_builder
    {
        public building_for_implicit_projects_without_source_folder()
        {
            given_working_dir_without_source_folder();
            given_an_existing_file_in_working_dir("test1.csproj");

            when_building_package();
        }

        [Test]
        public void soource_folder_is_not_required()
        {
            SrcRelatedErrorResults.ShouldNotBeEmpty();
        }
    }
    public class building_for_explicit_projects_without_source_folder : contexts.msbuild_builder
    {
        public building_for_explicit_projects_without_source_folder()
        {
            given_working_dir_without_source_folder();
            given_an_existing_file_in_working_dir("test1.csproj");

            when_building_package_for("test1.csproj");
        }
        [Test]
        public void source_folder_is_required()
        {
            SrcRelatedErrorResults.ShouldBeEmpty();
        }
    }
    namespace contexts
    {
        public abstract class msbuild_builder
        {
            IEnumerable<BuildResult> _buildResults;
            IFileSystem _fileSystem;
            IDirectory _testDir;

            protected IEnumerable<ErrorBuildResult> SrcRelatedErrorResults
            {
                get
                {
                    return _buildResults.OfType<ErrorBuildResult>().Where(r => r.Message != null && r.Message.Contains("src"));
                }
            }


            public msbuild_builder()
            {
                _fileSystem = new InMemoryFileSystem();
                _testDir = _fileSystem.CreateDirectory("testdir");
            }

            protected void given_working_dir_without_source_folder()
            {
                _testDir.GetFile("test.wrapdesc").MustExist();
            }

            protected void given_an_existing_file_in_working_dir(string fileName)
            {
                _testDir = _fileSystem.CreateDirectory("testdir");
                _testDir.GetFile(fileName).MustExist();
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