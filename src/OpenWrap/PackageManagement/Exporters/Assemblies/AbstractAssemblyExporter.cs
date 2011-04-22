using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using OpenWrap.Collections;
using OpenWrap.IO;
using OpenWrap.PackageModel;

namespace OpenWrap.PackageManagement.Exporters.Assemblies
{
    public abstract class AbstractAssemblyExporter : IExportProvider
    {
        protected string _platform;
        protected string _profile;
        const string ANYCPU = "AnyCPU";
        readonly string _exportName;

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
                    .Where(x=>!ReferenceEquals(x,null))
                    .GroupBy(x => x.Path);
        }

        static bool CanUseBinFolderForProfile(string binFolder, string profile)
        {
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

        static bool MatchesReferenceSection(Exports.IAssembly assembly)
        {
            if (assembly.Package.Descriptor == null || assembly.Package.Descriptor.ReferencedAssemblies == null) return true;
            var specs = assembly.Package.Descriptor.ReferencedAssemblies.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(spec => spec.Trim().Wildcard());
            var fileName = assembly.File.Name;
            return specs.Any(spec => spec.IsMatch(assembly.AssemblyName.Name) || spec.IsMatch(fileName));
        }

        static bool PlatformMatches(string binPlatform, string envPlatform)
        {
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