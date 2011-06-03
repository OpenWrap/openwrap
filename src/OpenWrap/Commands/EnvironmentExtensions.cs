using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.PackageManagement;
using OpenWrap.Runtime;

namespace OpenWrap.Commands
{
    public static class EnvironmentExtensions
    {
        public static IEnumerable<IGrouping<string, Exports.ICommand>> CommandExports(this IPackageManager manager, IEnvironment environment)
        {
            var projectCommands = environment.Descriptor != null && environment.ProjectRepository != null
                                      ? manager.GetProjectExports<Exports.ICommand>(environment.Descriptor, environment.ProjectRepository, environment.ExecutionEnvironment).SelectMany(x => x).GroupBy(
                                          x => x.Package.Name)
                                      : Enumerable.Empty<IGrouping<string, Exports.ICommand>>();

            var consumedPackages = projectCommands.Select(x => x.Key).ToList();
            var systemCommands = manager.GetSystemExports<Exports.ICommand>(environment.SystemRepository, environment.ExecutionEnvironment).SelectMany(x => x).GroupBy(x => x.Package.Name);
            return projectCommands.Concat(systemCommands.Where(x => consumedPackages.ContainsNoCase(x.Key) == false));
        }

        public static IEnumerable<ICommandDescriptor> Commands(this IPackageManager manager, IEnvironment environment)
        {
            return manager.CommandExports(environment).SelectMany(_ => _).Select(x => x.Descriptor);
        }
    }
}