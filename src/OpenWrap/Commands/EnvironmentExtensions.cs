using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.Collections;
using OpenWrap.PackageManagement;
using OpenWrap.PackageManagement.Exporters;
using OpenWrap.Runtime;

namespace OpenWrap.Commands
{
    public static class EnvironmentExtensions
    {
        public static IEnumerable<ICommandDescriptor> Commands(this IEnvironment environment)
        {
            var packageExporter = Services.ServiceLocator.GetService<IPackageExporter>();
            if (packageExporter == null)
                throw new InvalidOperationException("A package exporter service hasn't been found.");

            return packageExporter
                    .GetExports<IExport>("commands", environment.ExecutionEnvironment, new[] { environment.ProjectRepository, environment.SystemRepository }.NotNull())
                    .SelectMany(x => x.Items)
                    .OfType<ICommandExportItem>()
                    .Select(x => x.Descriptor)
                    .ToList();
        }
    }
}