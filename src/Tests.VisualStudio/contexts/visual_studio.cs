using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
using OpenWrap.Commands.Core;
using OpenWrap.Commands.Wrap;
using OpenWrap.Configuration;
using OpenWrap.Configuration.Remotes;
using OpenWrap.IO.Packaging;
using OpenWrap.ProjectModel.Drivers.File;
using OpenWrap.Services;
using OpenWrap.Testing;
using OpenWrap.VisualStudio;
using OpenWrap.VisualStudio.Hooks;
using OpenWrap.VisualStudio.SolutionAddIn;
using Tests.ProjectModel.drivers.file;
using Tests.VisualStudio.Artifacts;


namespace Tests.VisualStudio.contexts
{
    public class visual_studio : context, IDisposable
    {
        protected DTE2 Dte;
        protected List<VisualStudioError> Errors = new List<VisualStudioError>();
        protected int ExitCode;
        protected IDirectory OutDir;
        protected string Output;
        protected IDirectory RootDir;
        protected IFile SlnFile;
        protected bool Timeout;
        readonly IDirectory ConfigDir;
        readonly LocalFileSystem FileSystem;
        readonly List<Func<PackageContent>> OpenWrapAssemblies = new List<Func<PackageContent>>();
        readonly IFile OpenWrapPackage;
        readonly IDirectory SourceDir;
        readonly IDirectory SysRepoDir;
        readonly List<string> _commands = new List<string>();
        readonly ITemporaryDirectory _tempDir;
        string Vs2008Path;
        string Vs2010Path;
        Action<DTE2>[] _dteActions;
        ClassLibraryProject _project;
        bool _solutionHasAddin;
        string _solutionName;


        public visual_studio()
        {
            ServiceLocator.Clear();
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

        public void Dispose()
        {
            if (Dte.Solution.IsOpen) Dte.Solution.Close(CloseOptions.Save | CloseOptions.Wait);

            Dte.Application.Quit();
            Dte = null;
            MessageFilter.Revoke();
        }

        protected void given_command(string command)
        {
            _commands.Add(command);
        }

        protected void given_empty_solution_addin_com_reg()
        {
            AddInInstaller.Uninstall();
        }

        protected void given_file(string fileName, string content)
        {
            _project.AddCompile(fileName, content);
        }

        protected void given_openwrap_assemblyOf<T>(string destination)
        {
            OpenWrapAssemblies.Add(() => AssemblyOf<T>(destination));
        }

        protected void given_project_2010(string projectName)
        {
            _project = new ClassLibraryProject(projectName, projectName, projectName, OutDir.Path);
        }

        protected void given_solution_addin_com_reg()
        {
            AddInInstaller.Install();
        }

        protected void given_solution_file(string solutionName, bool withAddin = false)
        {
            _solutionName = solutionName;
            _solutionHasAddin = withAddin;
        }

        protected void given_vs_action(params Action<DTE2>[] actions)
        {
            _dteActions = actions;
        }

        protected void when_executing_vs2010(bool waitOnPlugins = false)
        {
            CreateProjectFiles();

            BuildOpenWrapPackage();

            InitializeServices();

            ExecuteCommands();

            Type t = Type.GetTypeFromProgID("VisualStudio.DTE.10.0", true);
            object app = Activator.CreateInstance(t, true);
            Dte = (DTE2)app;
            MessageFilter.Register();

            Dte.Solution.Open(SlnFile.Path);
            while (Dte.Solution.IsOpen == false) Thread.Sleep(TimeSpan.FromSeconds(1));
            foreach (var action in _dteActions)
                action(Dte);
            if (waitOnPlugins)
                Dte.Windows.Output("OpenWrap").WaitForMessage(StartSolutionPlugin.SOLUTION_PLUGINS_STARTED);
            ReadErrors();
            ReadOpenWrapOutput();

            foreach (var error in Errors) Console.WriteLine("Error: " + error.Description);
        }

        protected void when_loading_solution_with_plugins()
        {
            when_executing_vs2010(true);
        }

        PackageContent AssemblyOf<T>(string relativePath = "bin-net35")
        {
            var assembly = typeof(T).Assembly;
            return new PackageContent { FileName = assembly.GetName().Name + ".dll", RelativePath = relativePath, Stream = () => File.OpenRead(assembly.Location) };
        }

        void BuildOpenWrapPackage()
        {
            Packager.NewFromFiles(OpenWrapPackage, GetOpenWrapPackageContent());
            foreach (var file in FileSystem.GetFile(GetType().Assembly.Location).Parent.GetDirectory("Artifacts").Files("*.wrap"))
                file.CopyTo(SysRepoDir.GetFile(file.Name));
        }

        void CreateProjectFiles()
        {
            SlnFile = SourceDir.GetFile(_solutionName);
            var sol = new SolutionFile(SlnFile, SolutionConstants.VisualStudio2010Version);
            if (_solutionHasAddin)
                sol.OpenWrapAddInEnabled = true;

            var projectFile = SourceDir.GetDirectory(_project.Name).GetFile(_project.Name + ".csproj");
            _project.Write(projectFile);
            sol.AddProject(projectFile);
            sol.Save();
        }

        void ExecuteCommands()
        {
            var executor = new ConsoleCommandExecutor(
                ServiceLocator.GetService<IEnumerable<ICommandLocator>>(),
                ServiceLocator.GetService<IEventHub>(),
                ServiceLocator.GetService<ICommandOutputFormatter>());
            foreach (var command in _commands)
                executor.Execute(command, Enumerable.Empty<string>());
        }

        IEnumerable<PackageContent> GetOpenWrapPackageContent()
        {
            yield return Static("openwrap.wrapdesc", ".", VsFiles.openwrap_wrapdesc);
            yield return Static("version", ".", Encoding.UTF8.GetBytes("99.99"));
            // bin-net35
            yield return AssemblyOf<ICommand>(); // openwrap.dll
            yield return AssemblyOf<IHttpClient>(); // openrasta.client.dll
            yield return AssemblyOf<SolutionAddInEnabler>(); // openwrap.visualstudio.shared.dll
            // build
            yield return Static("OpenWrap.CSharp.targets", "build", VsFiles.OpenWrap_CSharp_targets);
            yield return Static("OpenWrap.tasks", "build", VsFiles.OpenWrap_tasks);
            yield return AssemblyOf<InitializeOpenWrap>("build"); // openwrap.build.bootstrap.dll
            yield return AssemblyOf<RunCommand>("build"); // openwrap.build.tasks.dll
            yield return AssemblyOf<OpenWrapVisualStudioAddIn>(); // openwrap.visualstudio.comshim.dll

            // commands
            yield return AssemblyOf<BuildWrapCommand>("commands-net35");

            foreach (var added in OpenWrapAssemblies)
                yield return added();
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

        void ReadErrors()
        {
            for (int i = 1; i <= Dte.ToolWindows.ErrorList.ErrorItems.Count; i++)
            {
                var error = Dte.ToolWindows.ErrorList.ErrorItems.Item(i);
                Errors.Add(new VisualStudioError
                {
                    Description = TryGet(() => error.Description),
                    Project = TryGet(() => error.Project),
                    FileName = TryGet(() => error.FileName)
                });
            }
        }

        void ReadOpenWrapOutput()
        {
            Output = Dte.Windows.Output("OpenWrap").Read();
        }

        PackageContent Static(string fileName, string relativePath, byte[] content)
        {
            return new PackageContent { FileName = fileName, RelativePath = relativePath, Stream = () => new MemoryStream(content) };
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

        protected string resharper_assert(string assertionName)
        {
            var resharperTestWindow = Dte.Windows.Output("OpenWrap-Tests", true);
            resharperTestWindow.OutputString("?ReSharper" + assertionName + "\r\n");

            var response = "!ReSharper" + assertionName + ":";
            resharperTestWindow.WaitForMessage(response, waitFor: TimeSpan.FromMinutes(5));
            var returnValue = resharperTestWindow.Read()
                .Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Where(x => x.StartsWith(response))
                .Select(x => x.Substring(response.Length))
                .Last();
            resharperTestWindow.Clear();
            return returnValue;
        }
    }
}