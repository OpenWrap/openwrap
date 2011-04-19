using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using OpenFileSystem.IO;
using OpenWrap.IO;
using OpenWrap.PackageManagement.Exporters;
using OpenWrap.PackageModel;
using OpenWrap.Repositories;
using OpenWrap.Runtime;
using OpenWrap.Services;

namespace OpenWrap.PackageManagement
{
    public interface IPackageExporter : IService
    {
        ///// <summary>
        /////   Gets all the exports present in the provided repositories.
        ///// </summary>
        ///// <typeparam name = "T"></typeparam>
        ///// <param name = "exportName"></param>
        ///// <param name = "environment"></param>
        ///// <param name = "repositories"></param>
        ///// <returns></returns>
        //IEnumerable<T> GetExports<T>(string exportName, ExecutionEnvironment environment, IEnumerable<IPackageRepository> repositories)
        //        where T : IExport;

        IEnumerable<IGrouping<string, TItems>> Exports<TItems>(IPackage package) where TItems : IExportItem;
    }

    public interface IExportProvider
    {
        IEnumerable<IGrouping<string, TItem>> Items<TItem>(IPackage pacakge) where TItem : IExportItem;
    }

    public static class PackageExporterExtensions
    {
        public static IEnumerable<Exports.IFile> Content(this IPackageExporter exporter, IPackage package)
        {
            return package.Content.SelectMany(_ => _);
        }
        public static IEnumerable<Exports.IAssembly> Assemblies(this IPackageExporter exporter, IPackage package)
        {
            return exporter.Exports<Exports.IAssembly>(package).SelectMany(x => x);
        }
    }

    public abstract class AbstractAssemblyExporter : IExportProvider
    {
        const string ANYCPU = "AnyCPU";
        string _exportName;
        protected string _profile;
        protected string _platform;

        public AbstractAssemblyExporter(string export, string profile, string platform)
        {
            _exportName = export;
            _profile = profile;
            _platform = platform;
        }
        public virtual IEnumerable<IGrouping<string, TItem>> Items<TItem>(IPackage package) where TItem : IExportItem
        {
            if (typeof(TItem) != typeof(Exports.IAssembly)) return Enumerable.Empty<IGrouping<string, TItem>>();

            return GetAssemblies<TItem>(package);
        }

        protected IEnumerable<IGrouping<string, TItems>> GetAssemblies<TItems>(IPackage package) where TItems : IExportItem
        {
            var allAssemblies = from directory in package.Content
                                let packageExportName = directory.Key.EqualsNoCase(_exportName)
                                    ? _exportName + "-" + _platform + "-" + _profile
                                    : (directory.Key.StartsWithNoCase(_exportName + "-") ? directory.Key : null)
                                where packageExportName != null
                                let segments = packageExportName.Split(new[] { "-" }, StringSplitOptions.RemoveEmptyEntries)
                                let platform = segments.Length == 3 ? segments[1] : ANYCPU
                                let profile = segments.Length == 3 ? segments[2] : segments[1]
                                where (PlatformMatches(platform, _platform) &&
                                       CanUseBinFolderForProfile(profile, _profile))
                                from file in directory
                                where IsDotNetCode(file)
                                group new
                                {
                                    file,
                                    directory = directory.Key,
                                    env = new EnvironmentDependentFile { Platform = platform, Profile = profile }
                                } by file.File.Name;

            return allAssemblies
                    .Select(x => x.OrderBy(_ => _.env)
                        .Select(_ =>
                        {
                            var an = _.file.File.Read(s =>
                            {
                                var def = AssemblyDefinition.ReadAssembly(s, new ReaderParameters(ReadingMode.Deferred));
                                var cecilName = def.Name;
                                var attributes = AssemblyNameFlags.None;
                                if (cecilName.IsRetargetable)
                                    attributes |= AssemblyNameFlags.Retargetable;
                                if (cecilName.HasPublicKey)
                                    attributes |= AssemblyNameFlags.PublicKey;

                                return new AssemblyName(def.FullName)
                                {
                                        CodeBase = _.file.File.Path.FullPath,
                                        HashAlgorithm = cecilName.HashAlgorithm == AssemblyHashAlgorithm.SHA1
                                                                ? System.Configuration.Assemblies.AssemblyHashAlgorithm.SHA1
                                                                : System.Configuration.Assemblies.AssemblyHashAlgorithm.None,
                                        Flags = attributes
                                };
                            });
                            return new Assembly(
                                    _.file.Path,
                                    _.file.Package,
                                    _.file.File,
                                    _.env.Platform,
                                    _.env.Profile,
                                    an
                                    );
                        })
                        .Where(MatchesReferenceSection)
                        .Cast<TItems>()
                        .FirstOrDefault())
                    .GroupBy(x => x.Path);
        }

        static bool MatchesReferenceSection(Exports.IAssembly assembly)
        {
            if (assembly.Package.Descriptor == null || assembly.Package.Descriptor.ReferencedAssemblies == null) return true;
            var specs = assembly.Package.Descriptor.ReferencedAssemblies.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                                                               .Select(spec => spec.Trim().Wildcard());
            var fileName = assembly.File.Name;
            return specs.Any(spec => spec.IsMatch(assembly.AssemblyName.Name) || spec.IsMatch(fileName));
        }
        bool IsDotNetCode(Exports.IFile file)
        {
            return file.File.Extension.EqualsNoCase(".dll") || file.File.Extension.EqualsNoCase(".exe");
        }

        static bool PlatformMatches(string binPlatform, string envPlatform)
        {
            return binPlatform.EqualsNoCase(ANYCPU) || (envPlatform.EqualsNoCase(ANYCPU) == false && binPlatform.EqualsNoCase(envPlatform));
        }

        static bool CanUseBinFolderForProfile(string binFolder, string profile)
        {
            if (profile == "net40")
                return binFolder == "net40" || binFolder == "net40cp" || binFolder == "net35" || binFolder == "net35cp" || binFolder == "net30" || binFolder == "net20";

            if (profile == "net40cp")
                return binFolder == "net40cp" || binFolder == "net35cp" || binFolder == "net30" || binFolder == "net20";

            if (profile == "net35")
                return binFolder == "net35" || binFolder == "net35cp" || binFolder == "net30" || binFolder == "net20";
            if (profile == "net35cp")
                return binFolder == "net35cp" || binFolder == "net30" || binFolder == "net20";

            if (profile == "net30")
                return binFolder == "net30" || binFolder == "net20";
            if (profile == "net20")
                return binFolder == "net20";

            if (profile == "sl40")
                return binFolder == "sl40" || binFolder == "sl30" || binFolder == "sl20";
            if (profile == "sl30")
                return binFolder == "sl30" || binFolder == "sl20";
            if (profile == "sl20")
                return binFolder == "sl20";

            if (profile == "wp70")
                return binFolder == "wp70";

            if (profile == "monotouch20")
                return binFolder == "monotouch20";
            if (profile == "monodroid20")
                return binFolder == "monodroid20";
            return false;
        }
    }

    public class EnvironmentDependentAssemblyExporter : AbstractAssemblyExporter
    {
        public EnvironmentDependentAssemblyExporter(ExecutionEnvironment env)
            : base("bin", env.Profile, env.Platform)
        {
        }
    }
}