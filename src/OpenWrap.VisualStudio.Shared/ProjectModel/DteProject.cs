using System.Linq;
using EnvDTE;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.Local;
using OpenWrap.ProjectModel;
using OpenWrap.ProjectModel.Drivers.File;
using OpenWrap.Runtime;

namespace OpenWrap.VisualStudio.ProjectModel
{

    public class DteProject : IProject
    {
        const string MSBUILD_NS = "http://schemas.microsoft.com/developer/msbuild/2003";
        public Project DteObject { get; private set; }

        public DteProject(Project project)
        {
            DteObject = project;
            OpenWrapEnabled = MSBuildProject.OpenWrapEnabled(project.FullName);
            File = LocalFileSystem.Instance.GetFile(project.FullName);
        }
        public TargetFramework TargetFramework
        {
            get
            {
                var targetFramework = (uint)DteObject.Properties.Item("TargetFramework").Value;
                var monikerProperty = DteObject.Properties.OfType<Property>().FirstOrDefault(x=>x.Name=="TargetFrameworkMoniker");
                var targetMoniker = monikerProperty == null ? null : (string)monikerProperty.Value;
                return TargetFramework.ParseDTEIdentifier(targetFramework, targetMoniker);
            }
        }

        public string TargetPlatform
        {
            get { return (string)DteObject.ConfigurationManager.ActiveConfiguration.Properties.Item("PlatformTarget").Value; }
        }

        public bool OpenWrapEnabled { get; private set; }

        public IFile File { get; private set; }
    }
}