using System;
using System.Collections.Generic;
using OpenFileSystem.IO;
using OpenWrap.Dependencies;
using OpenWrap.PackageManagement;
using OpenWrap.Repositories;

namespace OpenWrap.Commands.Wrap
{
    public abstract class WrapCommand : AbstractCommand
    {
        public WrapCommand()
        {
            Successful = true;
        }
        public IEnvironment Environment
        {
            get { return Services.Services.GetService<IEnvironment>(); }

        }
        protected IPackageManager PackageManager
        {
            get { return Services.Services.GetService<IPackageManager>(); }
        }
        protected IPackageResolver PackageResolver
        {
            get { return Services.Services.GetService<IPackageResolver>(); }
        }

        protected IPackageExporter PackageExporter
        {
            get { return Services.Services.GetService<IPackageExporter>(); }
        }
        protected IPackageDeployer PackageDeployer
        {
            get { return Services.Services.GetService<IPackageDeployer>(); }
        }

        protected DependencyResolutionResult ResolveDependencies(PackageDescriptor packageDescriptor, IEnumerable<IPackageRepository> repos)
        {
            return PackageResolver.TryResolveDependencies(packageDescriptor, repos);
        }

        protected virtual ICommandOutput ToOutput(PackageOperationResult packageOperationResult)
        {
            var output = packageOperationResult.ToOutput();
            this.Successful = this.Successful && (output.Type == CommandResultType.Info || output.Type == CommandResultType.Data || output.Type == CommandResultType.Verbose);
            return output;
        }

        protected bool Successful { get; private set; }


        protected void TrySaveDescriptorFile()
        {
            if (!Successful) return;
            using (var descriptor = Environment.DescriptorFile.OpenWrite())
                new PackageDescriptorReaderWriter().Write(Environment.Descriptor, descriptor);
        }
    }
}