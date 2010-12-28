using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.Local;
using OpenWrap.Build;
using OpenWrap.Build.PackageBuilders;
using OpenWrap.PackageModel;
using OpenWrap.Repositories;
using OpenWrap.Runtime;
using OpenWrap.Tests.Build.package_builder_specs.context;
using Path = System.IO.Path;

namespace OpenWrap.Tests.Build.package_builder_specs
{
    public class msbuild_builder_spec : msbuild_builder
    {
        [Test]
        public void doesnt_require_src_folder_for_explicit_projects()
        {
            given_a_working_dir_without_src_folder();
            given_an_existing_file_in_working_dir("test1.csproj");

            when_building_package_for("test1.csproj");

            should_not_contain_error_about_src();
        }

        [Test]
        public void does_require_src_folder_for_implicit_projects()
        {
            given_a_working_dir_without_src_folder();
            given_an_existing_file_in_working_dir("test1.csproj");

            when_building_package();

            should_contain_error_about_src();
        }
    }

    namespace context
    {
        public abstract class msbuild_builder
        {
            IEnumerable<BuildResult> _buildResults;
            DirectoryInfo _testDir;

            [TearDown]
            public void Terminate()
            {
                EnsureDirDoesNotExist(_testDir);
            }

            static void EnsureDirDoesNotExist(DirectoryInfo directoryInfo)
            {
                if (directoryInfo == null || !directoryInfo.Exists) return;
                Directory.Delete(directoryInfo.FullName, true);
                directoryInfo.Refresh();
            }

            static void EnsureFileExists(FileInfo f)
            {
                if (f.Exists) return;
                using (var writer = f.CreateText())
                {
                    writer.Close();
                }
            }

            protected void given_a_working_dir_without_src_folder()
            {
                EnsureDirDoesNotExist(_testDir);
                var tempfile = Path.GetTempFileName();
                File.Delete(tempfile);
                _testDir = new DirectoryInfo(tempfile);
                _testDir.Create();

                EnsureFileExists(new FileInfo(Path.Combine(_testDir.FullName, "test.wrapdesc")));
            }

            protected void given_an_existing_file_in_working_dir(string fileName)
            {
                Assert.IsNotNull(_testDir, "Working dir was not initialized");
                Assert.IsTrue(_testDir.Exists, "Working dir does not exist: {0}", _testDir.FullName);
                var f = new FileInfo(Path.Combine(_testDir.FullName, fileName));
                EnsureFileExists(f);
            }

            protected void should_not_contain_error_about_src()
            {
                Assert.IsNotNull(_buildResults, "Build results were not initialized");
                var srcErrors = SrcRelatedErrorResults.ToList();
                Assert.IsEmpty(srcErrors, "Got build error: {0}", (srcErrors.FirstOrDefault() ?? new ErrorBuildResult("no error?!")).Message);
            }

            protected void should_contain_error_about_src()
            {
                Assert.IsNotNull(_buildResults, "Build results were not initialized");
                var srcErrors = SrcRelatedErrorResults.ToList();
                Assert.IsNotEmpty(srcErrors, "Got build error: {0}", (srcErrors.FirstOrDefault() ?? new ErrorBuildResult("no error?!")).Message);
            }

            IEnumerable<ErrorBuildResult> SrcRelatedErrorResults
            {
                get
                {
                    return (from r in _buildResults
                            where r is ErrorBuildResult && r.Message != null && r.Message.Contains("src")
                            select (ErrorBuildResult)r);
                }
            }

            protected void when_building_package_for(string project)
            {
                var builder = new MSBuildPackageBuilder(LocalFileSystem.Instance, new EnvironmentStub(_testDir), new ResultParserStub()) { Project = new[] { project } };
                _buildResults = builder.Build();
            }

            protected void when_building_package()
            {
                var builder = new MSBuildPackageBuilder(LocalFileSystem.Instance, new EnvironmentStub(_testDir), new ResultParserStub());
                _buildResults = builder.Build();
            }

            class EnvironmentStub : IEnvironment
            {
                readonly DirectoryInfo _testDir;

                public EnvironmentStub(DirectoryInfo testDir)
                {
                    if (testDir == null) throw new ArgumentNullException("testDir");
                    _testDir = testDir;
                }

                public IDirectory ConfigurationDirectory
                {
                    get { throw new NotSupportedException(); }
                }

                public IDirectory CurrentDirectory
                {
                    get { return LocalFileSystem.Instance.GetDirectory(_testDir.FullName); }
                }

                public IPackageRepository CurrentDirectoryRepository
                {
                    get { throw new NotSupportedException(); }
                }

                public PackageDescriptor Descriptor
                {
                    get { throw new NotSupportedException(); }
                }

                public IFile DescriptorFile
                {
                    get { return CurrentDirectory.GetFile("test.wrapdesc"); }
                }

                public ExecutionEnvironment ExecutionEnvironment
                {
                    get { return new ExecutionEnvironment("AnyCPU", "net-3.5"); }
                }

                public IPackageRepository ProjectRepository
                {
                    get { throw new NotSupportedException(); }
                }

                public IEnumerable<IPackageRepository> RemoteRepositories
                {
                    get { throw new NotSupportedException(); }
                }

                public IPackageRepository SystemRepository
                {
                    get { throw new NotSupportedException(); }
                }

                public void Initialize()
                {
                    throw new NotSupportedException();
                }
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