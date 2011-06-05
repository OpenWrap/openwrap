using System;
using System.Collections.Generic;
using System.Linq;
using OpenFileSystem.IO;
using OpenWrap.Collections;
using OpenWrap.Commands.Remote.Messages;
using OpenWrap.Configuration;
using OpenWrap.Configuration.Remotes;
using OpenWrap.PackageManagement;
using OpenWrap.PackageManagement.DependencyResolvers;
using OpenWrap.PackageModel;
using OpenWrap.PackageModel.Serialization;
using OpenWrap.Repositories;
using OpenWrap.Runtime;
using OpenWrap.Services;

namespace OpenWrap.Commands.Wrap
{
    public abstract class WrapCommand : AbstractCommand
    {
        protected WrapCommand()
        {
            Successful = true;
        }

        public IEnvironment HostEnvironment
        {
            get { return ServiceLocator.GetService<IEnvironment>(); }
        }

        protected IConfigurationManager ConfigurationManager
        {
            get { return ServiceLocator.GetService<IConfigurationManager>(); }
        }
        protected IRemoteManager Remotes { get
        {
            return ServiceLocator.GetService<IRemoteManager>();
        }}

        protected IFileSystem FileSystem
        {
            get { return ServiceLocator.GetService<IFileSystem>(); }
        }

        protected IPackageDeployer PackageDeployer
        {
            get { return ServiceLocator.GetService<IPackageDeployer>(); }
        }

        protected IPackageExporter PackageExporter
        {
            get { return ServiceLocator.GetService<IPackageExporter>(); }
        }

        protected IPackageManager PackageManager
        {
            get { return ServiceLocator.GetService<IPackageManager>(); }
        }

        protected IPackageResolver PackageResolver
        {
            get { return ServiceLocator.GetService<IPackageResolver>(); }
        }

        protected IEnumerable<IRemoteRepositoryFactory> RemoteFactories
        {
            get { return ServiceLocator.GetService<IEnumerable<IRemoteRepositoryFactory>>(); }
        }

        protected bool Successful { get; private set; }

        protected IDisposable ChangeMonitor(FileBased<IPackageDescriptor> descriptor)
        {
            bool packagesChanged = false;
            PackageChanged change = (a, b, c, d) =>
            {
                packagesChanged = true;
                return Enumerable.Empty<object>();
            };
            PackageUpdated update = (a, b, c, d, e) =>
            {
                packagesChanged = true;
                return Enumerable.Empty<object>();
            };
            var listener = PackageManager.Monitor(change, change, update);
            return new ActionOnDispose(() =>
            {
                listener.Dispose();
                if (packagesChanged)
                {
                    if (HostEnvironment.Descriptor != descriptor.Value)
                    {
                        descriptor.File.Touch();
                        HostEnvironment.DescriptorFile.Touch();
                    }
                    else
                    {
                        foreach (var file in HostEnvironment.ScopedDescriptors.Select(x => x.Value.File))
                            file.Touch();
                    }
                }
            });
        }

        protected IEnumerable<ICommandOutput> HintRemoteRepositories()
        {
            yield return new Info("The list of configured repositories can be seen using the 'list-remote' command. The currently configured repositories are:");
            foreach (var m in ConfigurationManager.Load<RemoteRepositories>()
                .OrderBy(x => x.Value.Priority)
                .Select(x => new RemoteRepositoryData(x.Value.Priority, x.Value.Name, x.Value.PublishRepositories.Count > 0, x.Value.FetchRepository != null)))
                yield return m;
        }

        protected DependencyResolutionResult ResolveDependencies(PackageDescriptor packageDescriptor, IEnumerable<IPackageRepository> repos)
        {
            return PackageResolver.TryResolveDependencies(packageDescriptor, repos);
        }

        protected virtual ICommandOutput ToOutput(PackageOperationResult packageOperationResult)
        {
            var output = packageOperationResult.ToOutput();
            Successful = Successful && (output.Type != CommandResultType.Error);
            return output;
        }

        protected void TrySaveDescriptorFile(FileBased<IPackageDescriptor> targetDescriptor)
        {
            if (!Successful) return;
            using (var descriptorStream = targetDescriptor.File.OpenWrite())
                new PackageDescriptorReaderWriter().Write(targetDescriptor.Value, descriptorStream);
        }
    }
}