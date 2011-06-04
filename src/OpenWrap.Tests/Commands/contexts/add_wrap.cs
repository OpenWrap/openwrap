using OpenFileSystem.IO;
using OpenWrap.Commands.contexts;
using OpenWrap.Commands.Wrap;
using OpenWrap.IO.Packaging;
using OpenWrap.PackageModel;
using OpenWrap.PackageModel.Serialization;
using OpenWrap.Repositories;

namespace Tests.Commands.contexts
{
    public class add_wrap : command<AddWrapCommand>
    {
        protected IDirectory ProjectRepositoryDir;
        protected IPackageDescriptor PostExecutionDescriptor;

        public add_wrap()
        {
            given_remote_repository("iron-hills");
        }
        protected void when_executing_command(string parameters)
        {
            base.when_executing_command(parameters);
            
            PostExecutionDescriptor = new PackageDescriptorReaderWriter().Read(Environment.DescriptorFile);
        }

        protected void given_file_based_project_repository()
        {
            ProjectRepositoryDir = FileSystem.GetDirectory(@"c:\repo");
            given_project_repository(new FolderRepository(ProjectRepositoryDir, FolderRepositoryOptions.UseSymLinks | FolderRepositoryOptions.AnchoringEnabled));
        }

        protected void given_file_package(string directory, string name, string version, params string[] lines)
        {
            Packager.NewWithDescriptor(FileSystem.GetDirectory(directory).GetFile(PackageNameUtility.PackageFileName(name, version)), name, version, lines);
        }
    }
}