using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using ConsoleApplication2;
using EnvDTE;
using EnvDTE80;
using Microsoft.Win32;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.Local;
using OpenRasta.Client;
using OpenWrap.Build;
using OpenWrap.Build.Tasks;
using OpenWrap.Commands;
using OpenWrap.Commands.Cli;
using OpenWrap.Commands.Cli.Locators;
using OpenWrap.Commands.Wrap;
using OpenWrap.Configuration;
using OpenWrap.Configuration.Remotes;
using OpenWrap.IO.Packaging;
using OpenWrap.ProjectModel.Drivers.File;
using OpenWrap.Services;
using OpenWrap.VisualStudio.ComShim;
using OpenWrap.VisualStudio.Hooks;
using Tests.ProjectModel.drivers.file;
using Tests.VisualStudio.new_solution;

namespace Tests.VisualStudio.contexts
{
    
    public class visual_studio
    {
        LocalFileSystem FileSystem;
        IDirectory RootDir;
        IFile SlnFile;
        List<string> _commands = new List<string>();
        string Vs2010Path;
        string Vs2008Path;
        protected int ExitCode;
        IDirectory SysRepoDir;
        IFile OpenWrapPackage;
        IDirectory ConfigDir;
        ClassLibraryProject _project;
        string _solutionName;
        protected bool Timeout;
        IDirectory SourceDir;
        ITemporaryDirectory _tempDir;
        protected List<VisualStudioError> Errors = new List<VisualStudioError>();
        protected IDirectory OutDir;


        public visual_studio()
        {
            Vs2010Path = (string)Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\VisualStudio\10.0\Setup\VS\").GetValue("EnvironmentDirectory") + "devenv.com";
            Vs2008Path = (string)Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\VisualStudio\9.0\Setup\VS\").GetValue("EnvironmentDirectory") + "devenv.com";
            FileSystem = LocalFileSystem.Instance;
            _tempDir = FileSystem.CreateTempDirectory();
            Console.WriteLine("Temp path: " + _tempDir.Path);
            RootDir = _tempDir.GetDirectory("root").MustExist();
            OutDir = _tempDir.GetDirectory("outdir").MustExist();
            ConfigDir = _tempDir.GetDirectory("config").MustExist();
            SysRepoDir = _tempDir.GetDirectory("sys").MustExist();
            SourceDir = RootDir.GetDirectory("src");

            OpenWrapPackage = SysRepoDir.GetFile("openwrap-99.99.wrap");
        }
        protected void given_solution_file(string solutionName)
        {
            _solutionName = solutionName;
        }
        protected void given_project_2010(string projectName)
        {
            _project = new ClassLibraryProject(projectName, projectName, projectName, OutDir.Path);
        }

        protected void given_command(string command)
        {
            _commands.Add(command);
        }
        protected void when_building_with_vs2010()
        {
            CreateProjectFiles();

            BuildOpenWrapPackage();

            InitializeServices();

            ExecuteCommands();

            System.Type t = System.Type.GetTypeFromProgID("VisualStudio.DTE.10.0", true);
            DTE2 dte = (DTE2)System.Activator.CreateInstance(t, true);
            MessageFilter.Register();
            try
            {
                dte.Solution.Open(SlnFile.Path);
                while (dte.Solution.IsOpen == false) System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1));
                dte.Solution.SolutionBuild.Build(true);
                dte.ExecuteCommand("File.SaveAll");
                for (int i = 1; i <= dte.ToolWindows.ErrorList.ErrorItems.Count; i++)
                {
                    var error = dte.ToolWindows.ErrorList.ErrorItems.Item(i);
                    Errors.Add(new VisualStudioError
                    {
                        Description = TryGet(()=>error.Description),
                        Project = TryGet(() => error.Project),
                        FileName = TryGet(() => error.FileName)
                    });
                }
                dte.ExecuteCommand("File.Exit");
            }
            finally
            {
                MessageFilter.Revoke();
            }
            foreach (var error in Errors) Console.WriteLine(error.Description);
        }

        string TryGet(Func<string> project)
        {
            try
            {
                return project();
            }
            catch
            {
                return null;
            }
        }

        void InitializeServices()
        {
            new ServiceRegistry()
                .CurrentDirectory(RootDir.Path)
                .SystemRepositoryDirectory(SysRepoDir.Path)
                .ConfigurationDirectory(ConfigDir.Path)
                .Initialize();

            ServiceLocator.GetService<IConfigurationManager>().Save(new RemoteRepositories());
        }

        void CreateProjectFiles()
        {
            SlnFile = SourceDir.GetFile(_solutionName);
            var sol = new SolutionFile(SlnFile, SolutionConstants.VisualStudio2010Version);
            var projectFile = SourceDir.GetDirectory(_project.Name).GetFile(_project.Name + ".csproj");
            _project.Write(projectFile);
            sol.AddProject(projectFile);
            sol.Save();
        }

        void BuildOpenWrapPackage()
        {
            Packager.NewFromFiles(OpenWrapPackage, GetOpenWrapPackageContent());
            foreach (var file in FileSystem.GetFile(GetType().Assembly.Location).Parent.GetDirectory("Artifacts").Files("*.wrap"))
                file.CopyTo(SysRepoDir.GetFile(file.Name));
        }

        IEnumerable<PackageContent> GetOpenWrapPackageContent()
        {
            yield return Static("openwrap.wrapdesc", ".", Files.openwrap_wrapdesc);
            yield return Static("version", ".", Encoding.UTF8.GetBytes("99.99"));
            // bin-net35
            yield return AssemblyOf<ICommand>(); // openwrap.dll
            yield return AssemblyOf<IHttpClient>(); // openrasta.client.dll
            yield return AssemblyOf<SolutionAddIn>(); // openwrap.visualstudio.shared.dll
            // build
            yield return Static("OpenWrap.CSharp.targets", "build", Files.OpenWrap_CSharp_targets);
            yield return Static("OpenWrap.tasks", "build", Files.OpenWrap_tasks);
            yield return AssemblyOf<InitializeOpenWrap>("build"); // openwrap.build.bootstrap.dll
            yield return AssemblyOf<RunCommand>("build"); // openwrap.build.tasks.dll
            yield return AssemblyOf<OpenWrapVisualStudioAddIn>("build"); // openwrap.visualstudio.comshim.dll

            // commands
            yield return AssemblyOf<BuildWrapCommand>("commands-net35");
        }

        PackageContent AssemblyOf<T>(string relativePath = "bin-net35")
        {
            var assembly = typeof(T).Assembly;
            return new PackageContent { FileName = assembly.GetName().Name + ".dll", RelativePath = relativePath, Stream = () => File.OpenRead(assembly.Location) };
        }

        PackageContent Static(string fileName, string relativePath, byte[] content)
        {
            return new PackageContent { FileName = fileName, RelativePath = relativePath, Stream = () => new MemoryStream(content) };
        }

        void ExecuteCommands()
        {
            var executor = new ConsoleCommandExecutor(ServiceLocator.GetService<IEnumerable<ICommandLocator>>(), ServiceLocator.GetService<ICommandOutputRenderer>());
            foreach (var command in _commands)
                executor.Execute(command, Enumerable.Empty<string>());
        }

        protected void given_file(string fileName, string content)
        {
            _project.AddCompile(fileName, content);
        }
    }

    public class VisualStudioError
    {
        public string Description { get; set; }

        public string FileName { get; set; }

        public string Project { get; set; }
    }
}