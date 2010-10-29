using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using OpenWrap.Dependencies;

namespace OpenWrap.Build
{
    public static class Preloader
    {
        static bool _initialized = false;
        public static void PreloadDependencies(params string[] packageNames)
        {
            //System.Diagnostics.Debugger.Launch();
            if (_initialized)
                return;
            _initialized = true;
            var openwrapAssemblyPath = typeof(InitializeOpenWrap).Assembly.Location;
            // openwrap is in /wraps/openwrap/build/ or /wraps/_cache/openwrap-xx/build
            var path = new DirectoryInfo(Path.GetDirectoryName(openwrapAssemblyPath));
            var rootWrapsPath = path.FullName.Contains("_cache")
                                        ? path.Parent.Parent
                                        : path.Parent.Parent.GetDirectories("_cache").FirstOrDefault();

            if (rootWrapsPath == null || !rootWrapsPath.Exists)
                throw new DirectoryNotFoundException("Pacakge cache could not be found. Cannot start OpenWrap.");


            var dependencyDirectories = packageNames.Select(x => GetDependencyDirectory(rootWrapsPath, x));
            foreach (var dependencyDirectory in dependencyDirectories)
            {
                if (dependencyDirectory == null || !dependencyDirectory.Exists)
                    throw new FileNotFoundException(string.Format("Package '{0}' could not be found. Cannot start OpenWrap.", dependencyDirectory));

                foreach (var assemblyFile in dependencyDirectory.GetFiles("*.dll"))
                {
                    try
                    {
                        Assembly.LoadFrom(assemblyFile.FullName);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        static DirectoryInfo GetDependencyDirectory(DirectoryInfo rootWrapsPath, string dependencyName)
        {
            var dir= (
                           from directory in rootWrapsPath.GetDirectories(dependencyName + "-*")
                           let version = PackageNameUtility.GetVersion(directory.Name)
                           orderby version descending
                           select directory
                   )
                   .FirstOrDefault();
            return dir.GetDirectories("bin-net35").First();
        }
    }
}