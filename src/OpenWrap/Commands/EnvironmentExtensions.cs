using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.Collections;
using OpenWrap.PackageManagement;
using OpenWrap.PackageManagement.Exporters;
using OpenWrap.Repositories;
using OpenWrap.Runtime;

namespace OpenWrap.Commands
{
    public static class EnvironmentExtensions
    {
        public static IEnumerable<ICommandDescriptor> Commands(this IEnvironment environment)
        {
            var packageExporter = Services.Services.GetService<IPackageExporter>();
            if (packageExporter == null)
                throw new InvalidOperationException("A package exporter service hasn't been found.");
            IEnumerable<ICommandDescriptor> commands = environment.ProjectRepository == null
                                                               ? Enumerable.Empty<ICommandDescriptor>()
                                                               : GetCommandsFor(environment, packageExporter, environment.ProjectRepository);
            return commands.Concat(GetCommandsFor(environment, packageExporter, environment.SystemRepository)).ToList();
        }

        static IEnumerable<ICommandDescriptor> GetCommandsFor(IEnvironment environment, IPackageExporter packageExporter, IPackageRepository repository)
        {
            try
            {
                return packageExporter
                        .GetExports<IExport>("commands", environment.ExecutionEnvironment, new[] { repository, environment.SystemRepository }.NotNull())
                        .SelectMany(x => x.Items)
                        .OfType<ICommandExportItem>()
                        .Select(x => x.Descriptor)
                        .ToList();
            }catch
            {
                return Enumerable.Empty<ICommandDescriptor>();
            }
        }
    }
}