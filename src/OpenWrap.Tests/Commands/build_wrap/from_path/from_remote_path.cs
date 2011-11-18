using OpenFileSystem.IO;

namespace Tests.Commands.build_wrap.from_path
{
    public class from_remote_path : contexts.build_wrap
    {
        protected IDirectory path_to_project;

        protected void given_remote_project()
        {
            path_to_project = FileSystem.CreateTempDirectory();
        }
    }
}