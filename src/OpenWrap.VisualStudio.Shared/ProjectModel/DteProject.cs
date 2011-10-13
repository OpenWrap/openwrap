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
        readonly LazyValue<IFile> _file;
        readonly LazyValue<bool> _openWrapEnabled;

        public DteProject(Project project)
        {
            DteObject = project;
            
            _openWrapEnabled = Lazy.Is(() => MSBuildProject.OpenWrapEnabled(project.FullName), true);
            _file = Lazy.Is(() => LocalFileSystem.Instance.GetFile(project.FullName));
        }


        public Project DteObject { get; private set; }

        public IFile File
        {
            get { return _file.Value; }
        }

        public bool OpenWrapEnabled
        {
            get { return _openWrapEnabled.Value; }
        }

        public TargetFramework TargetFramework
        {
            get
            {
                var targetFramework = (uint)DteObject.Properties.Item("TargetFramework").Value;
                var monikerProperty = DteObject.Properties.OfType<Property>().FirstOrDefault(x => x.Name == "TargetFrameworkMoniker");
                var targetMoniker = monikerProperty == null ? null : (string)monikerProperty.Value;
                return TargetFramework.ParseDTEIdentifier(targetFramework, targetMoniker);
            }
        }

        public string TargetPlatform
        {
            get { return (string)DteObject.ConfigurationManager.ActiveConfiguration.Properties.Item("PlatformTarget").Value; }
        }
    }
}