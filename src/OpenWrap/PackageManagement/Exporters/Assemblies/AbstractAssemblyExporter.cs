using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using OpenFileSystem.IO;
using OpenWrap.IO;
using OpenWrap.PackageModel;
using OpenWrap.Runtime;

namespace OpenWrap.PackageManagement.Exporters.Assemblies
{
    public abstract class AbstractAssemblyExporter : IExportProvider
    {
        const string ANYCPU = "AnyCPU";
        readonly string _exportName;

        public AbstractAssemblyExporter(string export)
        {
            _exportName = export;
        }

        public virtual IEnumerable<IGrouping<string, TItem>> Items<TItem>(IPackage package, ExecutionEnvironment environment) where TItem : IExportItem
        {
            if (typeof(TItem) != typeof(Exports.IAssembly)) return Enumerable.Empty<IGrouping<string, TItem>>();

            return GetAssemblies<TItem>(package, environment);
        }

        protected IEnumerable<IGrouping<string, TItems>> GetAssemblies<TItems>(IPackage package, ExecutionEnvironment executionEnvironment) where TItems : IExportItem
        {
            
            var allAssemblies = from directory in package.Content
                                let packageExportName = directory.Key.EqualsNoCase(_exportName)
                                                                ? _exportName + "-" + executionEnvironment.Platform + "-" + executionEnvironment.Profile
                                                                : (directory.Key.StartsWithNoCase(_exportName + "-") ? directory.Key : null)
                                where packageExportName != null
                                let segments = packageExportName.Split(new[] { "-" }, StringSplitOptions.RemoveEmptyEntries)
                                let platform = segments.Length == 3 ? segments[1] : ANYCPU
                                let profile = segments.Length == 3 ? segments[2] : segments[1]
                                where (PlatformMatches(platform, executionEnvironment.Platform) &&
                                       CanUseBinFolderForProfile(profile, executionEnvironment.Profile))
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
                                                     an,
                                                     GetAssemblyExportFlags(_.file, an));
                                         })
                                         .Where(HasExportFlags)
                                         .Cast<TItems>()
                                         .FirstOrDefault())
                    .Where(x=>!ReferenceEquals(x,null))
                    .GroupBy(x => x.Path);
        }

        static bool HasExportFlags(Assembly a)
        {
            return a.Flags != AssemblyExportFlags.None;
        }

        static AssemblyExportFlags GetAssemblyExportFlags(Exports.IFile file, AssemblyName an)
        {
            if (file.Package.Descriptor == null) return AssemblyExportFlags.None;

            var flags = MatchesReferenceSection(file, an, file.Package.Descriptor.ReferencedAssemblies) ? AssemblyExportFlags.ReferencedAssembly : AssemblyExportFlags.None;
            flags |= MatchesReferenceSection(file, an, file.Package.Descriptor.RuntimeAssemblies) ? AssemblyExportFlags.RuntimeAssembly : AssemblyExportFlags.None;
            return flags;
        }

        static bool CanUseBinFolderForProfile(string binFolder, string profile)
        {
            if (profile == "*") return true;
            if (profile.EqualsNoCase("net40"))
                return binFolder.EqualsNoCase("net40") || binFolder.EqualsNoCase("net40cp") || binFolder.EqualsNoCase("net35") || binFolder.EqualsNoCase("net35cp") || binFolder.EqualsNoCase("net30") ||
                       binFolder.EqualsNoCase("net20");

            if (profile.EqualsNoCase("net40cp"))
                return binFolder.EqualsNoCase("net40cp") || binFolder.EqualsNoCase("net35cp") || binFolder.EqualsNoCase("net30") || binFolder.EqualsNoCase("net20");

            if (profile.EqualsNoCase("net35"))
                return binFolder.EqualsNoCase("net35") || binFolder.EqualsNoCase("net35cp") || binFolder.EqualsNoCase("net30") || binFolder.EqualsNoCase("net20");
            if (profile.EqualsNoCase("net35cp"))
                return binFolder.EqualsNoCase("net35cp") || binFolder.EqualsNoCase("net30") || binFolder.EqualsNoCase("net20");

            if (profile.EqualsNoCase("net30"))
                return binFolder.EqualsNoCase("net30") || binFolder.EqualsNoCase("net20");
            if (profile.EqualsNoCase("net20"))
                return binFolder.EqualsNoCase("net20");

            if (profile.EqualsNoCase("sl50"))
                return binFolder.EqualsNoCase("sl50") || binFolder.EqualsNoCase("sl40") || binFolder.EqualsNoCase("sl30") || binFolder.EqualsNoCase("sl20");

            if (profile.EqualsNoCase("sl40"))
                return binFolder.EqualsNoCase("sl40") || binFolder.EqualsNoCase("sl30") || binFolder.EqualsNoCase("sl20");
            if (profile.EqualsNoCase("sl30"))
                return binFolder.EqualsNoCase("sl30") || binFolder.EqualsNoCase("sl20");
            if (profile.EqualsNoCase("sl20"))
                return binFolder.EqualsNoCase("sl20");

            if (profile.EqualsNoCase("wp70"))
                return binFolder.EqualsNoCase("wp70");

            if (profile.EqualsNoCase("monotouch20"))
                return binFolder.EqualsNoCase("monotouch20");
            if (profile.EqualsNoCase("monodroid20"))
                return binFolder.EqualsNoCase("monodroid20");
            return false;
        }

        static bool MatchesReferenceSection(Exports.IFile file, AssemblyName assemblyName, string list)
        {
            if (list == null) return true;
            var specs = list.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(spec => spec.Trim().Wildcard());
            var fileName = file.File.Name;
            return specs.Any(spec => spec.IsMatch(assemblyName.Name) || spec.IsMatch(fileName));
        }

        static bool PlatformMatches(string binPlatform, string envPlatform)
        {
            if (envPlatform == "*") return true;
            return binPlatform.EqualsNoCase(ANYCPU) || (envPlatform.EqualsNoCase(ANYCPU) == false && binPlatform.EqualsNoCase(envPlatform));
        }

        bool IsDotNetCode(Exports.IFile file)
        {
            return file.File.Extension.EqualsNoCase(".dll") || file.File.Extension.EqualsNoCase(".exe");
        }

        public class EnvironmentDependentFile : IComparable<EnvironmentDependentFile>
        {
            public IExportItem Item;
            public string Platform;
            public string Profile;

            public int CompareTo(EnvironmentDependentFile other)
            {
                if (this.Profile != other.Profile)
                {
                    return this.Profile.CompareTo(other.Profile) * -1;
                }
                if (Profile == other.Profile && Platform == other.Platform) return 0;
                if (Platform.EqualsNoCase(ANYCPU) == false) return -1;
                return 1;
            }
        }
    }
}