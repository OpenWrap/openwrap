using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenWrap.Repositories;
using OpenWrap.Dependencies;

namespace OpenWrap.Exports
{
    public class AssemblyReferenceExportBuilder : IExportBuilder
    {
        private const string ANYCPU = "AnyCPU";

        public string ExportName
        {
            get { return "bin"; }
        }

        public bool CanProcessExport(string exportName)
        {
            return exportName.StartsWith("bin", StringComparison.OrdinalIgnoreCase);
        }

        public IExport ProcessExports(IEnumerable<IExport> exports, ExecutionEnvironment environment)
        {
            var parsedExports = (from export in exports
                                 let exportSegments =
                                     export.Name.Split(new[] {"-"}, StringSplitOptions.RemoveEmptyEntries)
                                 let platform = exportSegments.Length == 3 ? exportSegments[1] : ANYCPU
                                 let target = exportSegments.Length == 3 ? exportSegments[2] : exportSegments[1]
                                 where PlatformMatches(platform, environment.Platform)
                                       && ProfileMatches(target, environment.Profile)
                                 from file in export.Items

                                 select new EnvironmentDependentFile()
                                 {
                                     Platform = platform,
                                     Profile = target,
                                     Item = file
                                 })
                .ToLookup(x => Path.GetFileName(x.Item.FullPath));

            // now for each assembly, find the most compatible
            var compatibleAssembly = parsedExports.Select(x =>
            {
                var ordered = x.ToList();
                ordered.Sort();
                var item =
                    ordered.Select(i => i.Item).FirstOrDefault();
                return item;
            });
            return new AssemblyReferenceExport(compatibleAssembly);
        }

        private bool ProfileMatches(string binProfile, string envProfile)
        {
            if (envProfile == "net35")
                return binProfile == "net35" || binProfile == "net30" || binProfile == "net20";
            if (envProfile == "net30")
                return binProfile == "net30" || binProfile == "net20";
            if (envProfile == "net20")
                return binProfile == "net20";
            return false;
        }

        private bool PlatformMatches(string binPlatform, string envPlatform)
        {
            return binPlatform == ANYCPU || (envPlatform != ANYCPU && binPlatform == envPlatform);
        }
    }
}