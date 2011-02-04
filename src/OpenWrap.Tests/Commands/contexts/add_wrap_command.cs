using OpenFileSystem.IO;
using OpenWrap.Commands.Wrap;
using OpenWrap.PackageModel;
using OpenWrap.PackageModel.Serialization;
using OpenWrap.Repositories;

namespace OpenWrap.Commands.contexts
{
    public class add_wrap_command : command_context<AddWrapCommand>
    {
        protected IDirectory ProjectRepositoryDir;
        protected IPackageDescriptor PostExecutionDescriptor;

        protected override void when_executing_command(params string[] parameters)
        {
            base.when_executing_command(parameters);

            PostExecutionDescriptor = new PackageDescriptorReaderWriter().Read(Environment.DescriptorFile);
        }

        protected void given_file_based_project_repository()
        {
            ProjectRepositoryDir = FileSystem.GetDirectory(@"c:\repo");
            given_project_repository(new FolderRepository(ProjectRepositoryDir, FolderRepositoryOptions.UseSymLinks | FolderRepositoryOptions.AnchoringEnabled));
        }
    }
    
}